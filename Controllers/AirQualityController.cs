using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using MetroAir.Models; // Ensure the Sensor model is accessible
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using UserRoles.Data;  // Reference to AppDbContext

namespace YourNamespace.Controllers
{
    [Route("api/airquality")]
    [ApiController]
    public class AirQualityController : ControllerBase
    {
        private static readonly string API_KEY = "afd0a6ef4a6ae0b9fd8a41474b30a4910fbe3b3e";
        private readonly ILogger<AirQualityController> _logger;
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public AirQualityController(ILogger<AirQualityController> logger, HttpClient httpClient, AppDbContext context)
        {
            _logger = logger;
            _httpClient = httpClient;
            _context = context;
        }

        private async Task<(int, string)> GetAQIData(int stationId)
        {
            try
            {
                var url = $"https://api.waqi.info/feed/@{stationId}/?token={API_KEY}";
                var response = await _httpClient.GetStringAsync(url);

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
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching AQI data: {ex.Message}");
                return (0, "Unknown");
            }
        }

        private string GetAQIStatus(int aqi)
        {
            return aqi switch
            {
                <= 50 => "Good",
                <= 100 => "Moderate",
                <= 150 => "Unhealthy for Sensitive Groups",
                <= 200 => "Unhealthy",
                <= 300 => "Very Unhealthy",
                _ => "Hazardous"
            };
        }





[HttpGet("/api/airquality/history/{sensorId}")]
public async Task<IActionResult> GetHistory(int sensorId)
{
    var historyData = await _context.AirQualityHistory
                                    .Where(h => h.SensorId == sensorId)
                                    .OrderByDescending(h => h.Timestamp)
                                    .ToListAsync();

    if (historyData == null || historyData.Count == 0)
    {
        return NotFound("No historical data found for the specified sensor.");
    }

    // Return formatted history data with Timestamp as ISO string
    var formattedData = historyData.Select(h => new
    {
        h.Id,
        h.SensorId,
        Timestamp = h.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), // Format to ISO 8601
        h.AQI,
        h.Status,
        h.PM2_5,
        h.PM10,
        h.NO2,
        h.SO2,
        h.CO,
        h.O3
    });

    return Ok(formattedData);
}




      

        [HttpGet]
public async Task<IActionResult> GetAirQualityData()
{
    // Define the manual locations with a strongly typed class
    var locations = new List<Location>
    {
        new Location { Id = 1, Name = "Battaramulla", Latitude = 6.9061, Longitude = 79.9189 },
        new Location { Id = 2, Name = "Pannipitiya", Latitude = 6.8444, Longitude = 79.9583 },
        new Location { Id = 3, Name = "Kuruduwatta", Latitude = 6.8801, Longitude = 79.8805 },
        new Location { Id = 4, Name = "Kirulapone", Latitude = 6.9082, Longitude = 79.8661 }
    };

    var responseList = new List<object>();

    foreach (var location in locations)
    {
        // Fetch the stationId for the current location from the database
     var station = await _context.Sensors
      .FirstOrDefaultAsync(s => s.Id == location.Id && s.Status == "Active");

            
        // If the stationId is found, use it, else skip this location
        if (station != null)
        {
            var aqiData = await GetAQIData(station.StationId);
            int aqi = aqiData.Item1;
            string status = aqiData.Item2;

            // Update the existing sensor data instead of adding new records to avoid duplicates
            station.AQI = aqi;
            station.AirQualityStatus = status;
            station.LastUpdated = DateTime.UtcNow;

            // Save the changes to the database
            _context.Sensors.Update(station);
            await _context.SaveChangesAsync();

            // Add air quality data to the response
            responseList.Add(new Dictionary<string, object>
            {
                { "id", location.Id },
                { "name", location.Name },
                { "latitude", location.Latitude },
                { "longitude", location.Longitude },
                { "aqi", aqi },
                { "airQualityStatus", status },
                { "pm25", aqi - 5 },  // Mock PM2.5 data
                { "pm10", aqi - 10 }, // Mock PM10 data
                { "temp", 30 + (location.Id % 5) },
                { "humidity", 50 + (location.Id * 2) },
                { "pressure", 1010 + location.Id },
                { "wind", 5.0 + (location.Id * 0.5) }
            });
        }
        else
        {
            _logger.LogWarning($"Station not found for location {location.Name}");
        }
    }

    return Ok(new { status = "ok", locations = responseList });
}

// Strongly typed Location class
// Strongly typed Location class
public class Location
{
    public int Id { get; set; } // Unique identifier for the location
    public string Name { get; set; } // Name of the location (e.g., Battaramulla)
    public double Latitude { get; set; } // Latitude coordinate for the location
    public double Longitude { get; set; } // Longitude coordinate for the location
    public int AQI { get; set; } // Air Quality Index for the location
    public string AirQualityStatus { get; set; } // Status of the air quality (e.g., Good, Moderate)
    public double? Pm25 { get; set; } // PM2.5 level, optional (for fine particulate matter)
    public double? Pm10 { get; set; } // PM10 level, optional (for coarse particulate matter)
    public double? Temp { get; set; } // Temperature at the location, optional
    public double? Humidity { get; set; } // Humidity at the location, optional
    public double? Pressure { get; set; } // Atmospheric pressure at the location, optional
    public double? Wind { get; set; } // Wind speed at the location, optional
}



    }
}
