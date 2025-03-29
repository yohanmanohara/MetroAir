using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using MetroAir.Models;
using MetroAir.ViewModels;
using UserRoles.Models;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using Microsoft.AspNetCore.Authorization;

namespace MetroAir.Controllers
{
    public class SensorController : Controller
    {
        private static readonly string API_KEY = "afd0a6ef4a6ae0b9fd8a41474b30a4910fbe3b3e";
        private readonly ILogger<SensorController> _logger;
        private readonly AppDbContext _context;

        public SensorController(ILogger<SensorController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateThresholds(AqiThresholdSettings model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/MonitoringAdmin/thresholdsettings.cshtml", model);
            }

            try
            {
                // Check if record exists
                var existingSettings = _context.AqiThresholdSettings.FirstOrDefault();

                if (existingSettings == null)
                {
                    // Create new record if none exists
                    model.Id = 1; // Ensure single record
                    _context.AqiThresholdSettings.Add(model);
                }
                else
                {
                    // Update existing record
                    existingSettings.GoodThreshold = model.GoodThreshold;
                    existingSettings.ModerateThreshold = model.ModerateThreshold;
                    existingSettings.UnhealthySensitiveThreshold = model.UnhealthySensitiveThreshold;
                    existingSettings.UnhealthyThreshold = model.UnhealthyThreshold;
                    existingSettings.VeryUnhealthyThreshold = model.VeryUnhealthyThreshold;
                    existingSettings.HazardousThreshold = model.HazardousThreshold;

                    _context.AqiThresholdSettings.Update(existingSettings);
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Threshold settings updated successfully!";
                return RedirectToAction("ThresholdSettings");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating thresholds: {ex.Message}");
                ModelState.AddModelError("", "Error updating thresholds: " + ex.Message);
                return View("~/Views/Home/MonitoringAdmin/thresholdsettings.cshtml", model);
            }
        }

        //[HttpGet]
        //public IActionResult GetCurrentThresholds()
        //{
        //    var settings = _thresholdService.GetCurrentThresholds();
        //    return Json(settings);
        //}



        [Authorize(Roles = "MonitoringAdmin")]
        [Route("Home/Overview")]
        [HttpGet]
        public async Task<IActionResult> GetSystemStatus()
        {
            try
            {
                // Get the data needed for both JSON and view responses
                var activeSensors = await _context.Sensors.CountAsync(s => s.Status == "Active");
                var totalSensors = await _context.Sensors.CountAsync();
                var settings = await _context.SimulationSettings.FirstOrDefaultAsync() ?? new SimulationSettings();
                var sensors = await _context.Sensors.ToListAsync();

                // If it's an AJAX request, return JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        activeSensors,
                        totalSensors,
                        isRunning = settings.IsRunning,
                        updateIntervalMinutes = settings.UpdateFrequencyMinutes
                    });
                }

                // For regular browser requests, return the view with all needed data
                var viewModel = new SystemStatusViewModel
                {
                    Sensors = sensors,
                    ActiveSensorsCount = activeSensors,
                    TotalSensorsCount = totalSensors,
                    IsRunning = settings.IsRunning,
                    UpdateIntervalMinutes = settings.UpdateFrequencyMinutes
                };

                return View("~/Views/Home/MonitoringAdmin/overview.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetSystemStatus: {ex.Message}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, new { error = "Error retrieving system status" });
                }

                return View("Error");
            }
        }





        [Authorize(Roles = "MonitoringAdmin")]
        [Route("Home/Overview/GetSensors")]
        [HttpGet]
        public async Task<IActionResult> GetSensors()
        {
            try
            {
                var sensors = await _context.Sensors
                    .Select(s => new
                    {
                        s.Id,
                        s.LocationName,
                        s.Status,
                        s.AQI,
                        s.LastUpdated
                    })
                    .ToListAsync();

                return Json(sensors);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting sensors: {ex.Message}");
                return StatusCode(500, new { error = "Error retrieving sensors" });
            }
        }


        [Authorize(Roles = "MonitoringAdmin")]
        [Route("Home/Overview/GetSensorHistory")]
        [HttpGet]

        public async Task<IActionResult> GetSensorHistory(int sensorId)
        {
            try
            {
                var history = await _context.AirQualityHistory
                    .Where(h => h.SensorId == sensorId)
                    .OrderByDescending(h => h.Timestamp)
                    .Select(h => new
                    {
                        h.Timestamp,
                        h.AQI,
                        h.Status,
                        h.PM2_5,
                        h.PM10,
                        h.NO2,
                        h.SO2,
                        h.CO,
                        h.O3
                    })
                    .ToListAsync();

                return Json(history);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting sensor history: {ex.Message}");
                return StatusCode(500, new { error = "Error retrieving sensor history" });
            }
        }

        //overview back-end end

        //[Authorize(Roles = "MonitoringAdmin")]
        //[HttpPost]
        //[Route("Home/oveview")]
        //public IActionResult GetActiveSensors()
        //{
        //    try
        //    {
        //        int activeSensors = _context.Sensors.Count(s => s.Status == "Active");
        //        return Ok(new { ActiveSensors = activeSensors });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = "Error retrieving sensor data", Error = ex.Message });
        //    }
        //}

        [Authorize(Roles = "MonitoringAdmin")]
        [HttpPost]
        [Route("Home/SensorManagement")]
        public IActionResult EditSensor(int id, string locationName)
        {
            var sensor = _context.Sensors.FirstOrDefault(s => s.Id == id);
            if (sensor != null)
            {
                sensor.LocationName = locationName;
                _context.Update(sensor);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(SensorManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(int id, string status)
        {
            var sensor = _context.Sensors.FirstOrDefault(s => s.Id == id);
            if (sensor != null)
            {
                sensor.Status = status == "Active" ? "Inactive" : "Active";
                _context.Update(sensor);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(SensorManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor == null)
            {
                return NotFound();
            }

            _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(SensorManagement));
        }

        [Authorize(Roles = "MonitoringAdmin")]
        [Route("Home/SensorManagement")]
        public async Task<IActionResult> SensorManagement()
        {
            var sensors = await _context.Sensors.ToListAsync();
            _logger.LogInformation($"Total sensors fetched: {sensors.Count}");

            foreach (var sensor in sensors)
            {
                var result = await GetAQIData(sensor.StationId);
                sensor.AQI = result.Item1;
                sensor.AirQualityStatus = result.Item2;
                _logger.LogInformation($"Sensor: {sensor.LocationName}, StationId: {sensor.StationId}, AQI: {sensor.AQI}, Status: {sensor.AirQualityStatus}");
            }

            return View("~/Views/Home/Monitoringadmin/sensormanagement.cshtml", sensors);
        }

        public IActionResult AddSensor(SensorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var sensors = _context.Sensors.ToList();
                return View("~/Views/Home/Monitoringadmin/sensormanagement.cshtml", sensors);
            }

            try
            {
                var newSensor = new Sensor
                {
                    LocationName = model.LocationName,
                    Type = model.Type,
                    StationId = model.StationId,
                    Status = model.Status,
                    CreatedDate = model.CreatedDate
                };

                _context.Sensors.Add(newSensor);
                _context.SaveChanges();
                return RedirectToAction(nameof(SensorManagement));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while adding sensor: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the sensor.");
                var sensors = _context.Sensors.ToList();
                return View("~/Views/Home/Monitoringadmin/sensormanagement.cshtml", sensors);
            }
        }

        private async Task<(int, string)> GetAQIData(int stationId)
        {
            try
            {
                var url = $"https://api.waqi.info/feed/@{stationId}/?token={API_KEY}";
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    _logger.LogInformation($"AQI API Response: {response}");
                    var data = JObject.Parse(response);

                    if (data["data"]?["aqi"] != null)
                    {
                        var aqi = data["data"]["aqi"].ToObject<int>();
                        return (aqi, GetAQIStatus(aqi));
                    }
                    _logger.LogError("AQI data is missing in API response.");
                    return (0, "Unknown");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching AQI data: {ex.Message}");
                return (0, "Unknown");
            }
        }

        private string GetAQIStatus(int aqi)
        {
            if (aqi <= 50) return "Good";
            if (aqi <= 100) return "Fair";
            if (aqi <= 150) return "Moderate";
            if (aqi <= 200) return "Poor";
            if (aqi <= 300) return "Very Poor";
            return "Hazardous";
        }
    }
}