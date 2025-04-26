using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UserRoles.Controllers
{
    [Authorize(Roles = "WebMaster")]
    [Route("Home/SystemConfiguration")]
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IConfiguration configuration,
            IWebHostEnvironment env,
            ILogger<AdminController> logger)
        {
            _configuration = configuration;
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Home/WebMaster/SystemConfiguration.cshtml");
        }

        [Route("GetBackupList")]
        [HttpGet]
        public IActionResult GetBackupList()
        {
            try
            {
                var backupPath = Path.Combine(_env.ContentRootPath, "Backups");

                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                    return Json(new List<object>());
                }

                var backups = Directory.GetFiles(backupPath, "*.bak")
                    .Select(file => new
                    {
                        fileName = Path.GetFileName(file),
                        created = System.IO.File.GetCreationTime(file),
                        size = new FileInfo(file).Length
                    })
                    .OrderByDescending(b => b.created)
                    .ToList();

                return Json(backups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backup list");
                return StatusCode(500, new { error = "Error getting backup list" });
            }
        }

        [Route("CreateBackup")]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                // Get backup directory
                var backupPath = Path.Combine(_env.ContentRootPath, "Backups");
                Directory.CreateDirectory(backupPath);

                // Get connection string from configuration
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    return BadRequest(new { success = false, message = "Database connection string not found" });
                }

                // Get database name
                var builder = new SqlConnectionStringBuilder(connectionString);
                var dbName = builder.InitialCatalog;
                
                if (string.IsNullOrWhiteSpace(dbName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Could not determine database name from connection string"
                    });
                }

                // Generate secure backup filename
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var backupFileName = $"{SanitizeFileName(dbName)}_Backup_{timestamp}.bak";
                var backupFilePath = Path.Combine(backupPath, backupFileName);

                // Execute backup
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Using parameterized query to prevent SQL injection
                    var backupCommand = @"BACKUP DATABASE @dbName TO DISK = @backupPath WITH FORMAT, 
                                   MEDIANAME = 'MetroAirBackups', 
                                   NAME = @backupName";

                    using (var command = new SqlCommand(backupCommand, connection))
                    {
                        command.Parameters.AddWithValue("@dbName", dbName);
                        command.Parameters.AddWithValue("@backupPath", backupFilePath);
                        command.Parameters.AddWithValue("@backupName", $"Full Backup of {dbName}");
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Verify backup was created
                if (!System.IO.File.Exists(backupFilePath) || new FileInfo(backupFilePath).Length == 0)
                {
                    throw new Exception("Backup file was not created or is empty");
                }

                return Json(new
                {
                    success = true,
                    fileName = backupFileName,
                    fileSize = new FileInfo(backupFilePath).Length,
                    message = "Backup created successfully"
                });
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error creating backup");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database backup failed",
                    sqlErrorNumber = sqlEx.Number,
                    sqlError = sqlEx.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating database backup");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Backup failed: {ex.Message}"
                });
            }
        }

        [Route("RestoreBackup")]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> RestoreBackup(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest(new { success = false, message = "Backup file name is required" });
                }

                // 1. Validate file exists
                var backupPath = Path.Combine(_env.ContentRootPath, "Backups");
                var filePath = Path.Combine(backupPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { success = false, message = "Backup file not found" });
                }

                // 2. Get connection string from configuration
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    return BadRequest(new { success = false, message = "Database connection string not found" });
                }

                // 3. Get database name
                var builder = new SqlConnectionStringBuilder(connectionString);
                var dbName = builder.InitialCatalog;
                
                if (string.IsNullOrWhiteSpace(dbName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Could not determine database name from connection string"
                    });
                }

                // 4. Execute restore
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Set database to single user mode
                    var singleUserCommand = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    using (var command = new SqlCommand(singleUserCommand, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                    try
                    {
                        // Execute restore with parameterized query
                        var restoreCommand = @"RESTORE DATABASE @dbName FROM DISK = @backupPath WITH REPLACE";

                        using (var command = new SqlCommand(restoreCommand, connection))
                        {
                            command.Parameters.AddWithValue("@dbName", dbName);
                            command.Parameters.AddWithValue("@backupPath", filePath);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    finally
                    {
                        // Set database back to multi user mode
                        var multiUserCommand = $"ALTER DATABASE [{dbName}] SET MULTI_USER";
                        using (var command = new SqlCommand(multiUserCommand, connection))
                        {
                            try
                            {
                                await command.ExecuteNonQueryAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to set database back to multi-user mode");
                            }
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Database restored successfully"
                });
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error restoring backup");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database restore failed",
                    sqlErrorNumber = sqlEx.Number,
                    sqlError = sqlEx.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring database");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Restore failed: {ex.Message}"
                });
            }
        }

        [Route("DownloadBackup")]
        [HttpGet]
        public IActionResult DownloadBackup(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".bak"))
                {
                    return BadRequest("Invalid backup file name");
                }

                var backupPath = Path.Combine(_env.ContentRootPath, "Backups");
                var filePath = Path.Combine(backupPath, fileName);

                // Security check - prevent path traversal attacks
                if (!filePath.StartsWith(backupPath) || !System.IO.File.Exists(filePath))
                {
                    return NotFound("Backup file not found");
                }

                var fileStream = System.IO.File.OpenRead(filePath);
                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading backup");
                return StatusCode(500, "Error downloading backup file");
            }
        }

        [Route("DeleteBackup")]
        [HttpDelete]
        public IActionResult DeleteBackup(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".bak"))
                {
                    return BadRequest(new { success = false, message = "Invalid backup file name" });
                }

                var backupPath = Path.Combine(_env.ContentRootPath, "Backups");
                var filePath = Path.Combine(backupPath, fileName);

                // Security check - prevent path traversal attacks
                if (!filePath.StartsWith(backupPath))
                {
                    return BadRequest(new { success = false, message = "Invalid backup file path" });
                }

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { success = false, message = "Backup file not found" });
                }

                System.IO.File.Delete(filePath);
                return Json(new { success = true, message = "Backup deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backup");
                return StatusCode(500, new { success = false, message = $"Error deleting backup: {ex.Message}" });
            }
        }

        // Helper method to sanitize filenames
        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        }
    }
}