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
        private static readonly string API_KEY = "afd0a6ef4a6ae0b9fd8a41474b30a4910fbe3b3e"; // Replace with your API Key
        private readonly ILogger<SensorController> _logger;
        private readonly AppDbContext _context;  // Injected DbContext to access database

        // Constructor to inject dependencies
        public SensorController(ILogger<SensorController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }




        [Authorize(Roles = "MonitoringAdmin")]
        [HttpPost]
        [Route("Home/overview")]
        public IActionResult GetActiveSensors()
        {
            try
            {
                int activeSensors = _context.Sensors.Count(s => s.Status == "Active");
                return Ok(new { ActiveSensors = activeSensors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving sensor data", Error = ex.Message });
            }
        }



        [Authorize(Roles = "MonitoringAdmin")]
        [HttpPost]
        [Route("Home/SensorManagement")]
        public IActionResult EditSensor(int id, string locationName)
        {
            var sensor = _context.Sensors.FirstOrDefault(s => s.Id == id);
            if (sensor != null)
            {
                sensor.LocationName = locationName;

                _context.Update(sensor);  // Update the sensor entity in the context
                _context.SaveChanges();  // Commit changes to the database
            }

            // Redirect to the SensorManagement view
            return RedirectToAction(nameof(SensorManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(int id, string status)
        {
            // Find the sensor based on its Id
            var sensor = _context.Sensors.FirstOrDefault(s => s.Id == id);
            if (sensor != null)
            {
                // Toggle the Status
                if (status == "Active")
                {
                    sensor.Status = "Inactive"; // Deactivate
                }
                else
                {
                    sensor.Status = "Active"; // Activate
                }

                // Save the changes to the database
                _context.Update(sensor);  // Update the status
                _context.SaveChanges();   // Save the changes
            }

            // Redirect to the SensorManagement view
            return RedirectToAction(nameof(SensorManagement));
        }


        // Delete Sensor
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

            // Redirect back to the SensorManagement view after deletion
            return RedirectToAction(nameof(SensorManagement));
        }




        // Route for Sensor Management
        [Authorize(Roles = "MonitoringAdmin")]
        [Route("Home/SensorManagement")]
        public async Task<IActionResult> SensorManagement()
        {
            
            var sensors = await _context.Sensors.ToListAsync();

           
            _logger.LogInformation($"Total sensors fetched: {sensors.Count}");

            
            foreach (var sensor in sensors)
            {
                // Assuming each sensor has a unique StationId
                var result = await GetAQIData(sensor.StationId);
                int aqi = result.Item1;  // Explicitly assign the first item in the tuple (AQI)
                string status = result.Item2;  // Explicitly assign the second item in the tuple (Status)

                sensor.AQI = aqi; // Assuming you have an AQI field in your Sensor model
                sensor.AirQualityStatus = status; // Assuming you have an AirQualityStatus field in your Sensor model
                _logger.LogInformation($"Sensor: {sensor.LocationName}, StationId: {sensor.StationId}, AQI: {aqi}, Status: {status}");
            }

            // Return the view with the sensors and AQI data
            return View("~/Views/Home/Monitoringadmin/sensormanagement.cshtml", sensors);
        }

        private async Task<(int, string)> GetAQIData(int stationId)
        {
            try
            {
                var apiKey = API_KEY; // Replace with your actual AQICN API key
                var url = $"https://api.waqi.info/feed/@{stationId}/?token={apiKey}";

                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    _logger.LogInformation($"AQI API Response: {response}"); // Log full response

                    var data = JObject.Parse(response);

                    if (data["data"] != null && data["data"]["aqi"] != null)
                    {
                        var aqi = data["data"]["aqi"].ToObject<int>();
                        string status = GetAQIStatus(aqi);
                        return (aqi, status);
                    }
                    else
                    {
                        _logger.LogError("AQI data is missing in API response.");
                        return (0, "Unknown");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching AQI data: {ex.Message}");
                return (0, "Unknown"); // Default in case of an error
            }
        }


        // Method to convert AQI value to status
        private string GetAQIStatus(int aqi)
        {
            if (aqi >= 0 && aqi <= 50) return "Good";
            if (aqi >= 51 && aqi <= 100) return "Fair";
            if (aqi >= 101 && aqi <= 150) return "Moderate";
            if (aqi >= 151 && aqi <= 200) return "Poor";
            if (aqi >= 201 && aqi <= 300) return "Very Poor";
            return "Hazardous"; // For AQI above 300
        }



        // Add Sensor
        public IActionResult AddSensor(SensorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var sensors = _context.Sensors.ToList(); // Get existing sensors from DB
                return View("~/Views/Home/Monitoringadmin/sensormanagement.cshtml", sensors);
            }

            try
            {
                
                var newSensor = new Sensor
                {
                    LocationName = model.LocationName,
                    Type = model.Type,
                    StationId=model.StationId,
                    Status = model.Status,
                    CreatedDate = model.CreatedDate
                };

                // Add the new sensor to the database and save changes
                _context.Sensors.Add(newSensor);
                _context.SaveChanges();

                // Redirect to the sensor management page after adding the new sensor
                return RedirectToAction(nameof(SensorManagement));
            }
            catch (Exception ex)
            {
                // Log the error (consider logging into a file or a monitoring system)
                _logger.LogError($"Error occurred while adding sensor: {ex.Message}");

                // Optionally return an error message to the view or show a friendly error page
                ModelState.AddModelError("", "An error occurred while saving the sensor.");
                var sensors = _context.Sensors.ToList(); // Get existing sensors from DB
                return View("~/Views/Home/Monitoringadmin/sensormanagement.cshtml", sensors);
            }
        }
    }
}
