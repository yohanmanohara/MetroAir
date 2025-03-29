using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserRoles.Models;
using UserRoles.ViewModels; 

namespace UserRoles.Controllers
{
    public class MonitoringUserManagementController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MonitoringUserManagementController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [Authorize(Roles = "WebMaster")]
        [Route("Home/SystemConfiguration")]
        public class AdminController : Controller
        {
            private readonly IConfiguration _configuration;
            private readonly IWebHostEnvironment _env;
            private readonly ILogger<AdminController> _logger;

            public AdminController(IConfiguration configuration,
                                 IWebHostEnvironment env,
                                 ILogger<AdminController> logger)
            {
                _configuration = configuration;
                _env = env;
                _logger = logger;
            }

            [Route("Home/SystemConfiguration/GetBackupList")]
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

            [Route("Home/SystemConfiguration/CreateBackup")]
            [HttpPost]
            [Produces("application/json")] // Ensures JSON response
            public async Task<IActionResult> CreateBackup()
            {
                try
                {
                    // 1. Validate and prepare backup directory
                    var backupPath = Path.Combine(_env.ContentRootPath, "Backups");
                    Directory.CreateDirectory(backupPath); // No need to check existence first

                    // 2. Get connection details (in production, get from config)
                    var connectionString = "Server=13.127.127.113;Database=nsbm;User=sa;Password=nsbm123!;Encrypt=True;TrustServerCertificate=True;";

                    // 3. Extract and validate database name
                    var dbName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
                    if (string.IsNullOrWhiteSpace(dbName))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Could not determine database name from connection string"
                        });
                    }

                    // 4. Generate secure backup filename
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var backupFileName = $"{SanitizeFileName(dbName)}_Backup_{timestamp}.bak";
                    var backupFilePath = Path.Combine(backupPath, backupFileName);

                    // 5. Execute backup with parameterized command
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        // Using parameterized query to prevent SQL injection
                        var backupCommand = @"BACKUP DATABASE @dbName TO DISK = @backupPath WITH FORMAT, 
                                MEDIANAME = 'SQLServerBackups', 
                                NAME = @backupName";

                        using (var command = new SqlCommand(backupCommand, connection))
                        {
                            command.Parameters.AddWithValue("@dbName", dbName);
                            command.Parameters.AddWithValue("@backupPath", backupFilePath);
                            command.Parameters.AddWithValue("@backupName", $"Full Backup of {dbName}");
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    // 6. Verify backup was created
                    if (!System.IO.File.Exists(backupFilePath) || new FileInfo(backupFilePath).Length == 0)
                    {
                        throw new Exception("Backup file was not created or is empty");
                    }

                    return Json(new
                    {
                        success = true,
                        fileName = backupFileName,
                        filePath = backupFilePath, // Consider if you want to expose this
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

            // Helper method to sanitize filenames
            private string SanitizeFileName(string fileName)
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                return new string(fileName
                    .Where(ch => !invalidChars.Contains(ch))
                    .ToArray());
            }
            [Route(" Home/SystemConfiguration/DownloadBackup")]
            [HttpGet]
            public IActionResult DownloadBackup(string fileName)
            {
                try
                {
                    var backupPath = Path.Combine(_env.ContentRootPath, "Backups");
                    var filePath = Path.Combine(backupPath, fileName);

                    if (!System.IO.File.Exists(filePath))
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

            [HttpDelete]
            public IActionResult DeleteBackup(string fileName)
            {
                try
                {
                    var backupPath = Path.Combine(_env.ContentRootPath, "Backups");
                    var filePath = Path.Combine(backupPath, fileName);

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

            private string GetDatabaseNameFromConnectionString(string connectionString)
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                return builder.InitialCatalog;
            }
        }

        // GET:
        [Authorize(Roles = "WebMaster")]
        [Route("Home/UserManagement")]
        public async Task<IActionResult> UserManagement()
        {
            var usersList = await _userManager.Users.ToListAsync(); // Fetch all users

            var users = new List<UserViewModel>();

            foreach (var u in usersList)
            {
                var roles = await _userManager.GetRolesAsync(u);
                users.Add(new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Roles = roles
                });
            }

            return View("~/Views/Home/WebMaster/UserManagement.cshtml", users);
        }

        // POST: UserManagement/Delete/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to delete user.");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string email, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "Invalid data.";
                return RedirectToAction("UserManagement");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UserManagement");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["Error"] = "Failed to remove existing roles.";
                return RedirectToAction("UserManagement");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                TempData["Error"] = "Failed to assign the new role.";
                return RedirectToAction("UserManagement");
            }

            TempData["Success"] = "User role updated successfully!";
            return RedirectToAction("UserManagement");
        }



        [HttpPost]
        public async Task<IActionResult> AddUser(string FullName, string Email, string Role, string Password, string ConfirmPassword)
        {
            // Check if all required fields are filled
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Role) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["Error"] = "All fields are required!";
                return RedirectToAction("UserManagement");
            }

            // Check if password and confirm password match
            if (Password != ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match!";
                return RedirectToAction("UserManagement");
            }

            // Create a new user
            var user = new Users
            {
                FullName = FullName,
                Email = Email,
                UserName = Email
            };

          
            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                // Assign the user to the selected role
                await _userManager.AddToRoleAsync(user, Role);
                TempData["Success"] = "User added successfully!";
            }
            else
            {
                TempData["Error"] = "User creation failed! " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("UserManagement");
        }

    }
    }