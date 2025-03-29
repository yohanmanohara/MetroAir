using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[Route("api/airquality")]
[ApiController]
public class AirQualityController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAirQualityData()
    {
        var data = new
        {
            status = "ok",
            locations = new List<object>
            {
                new {
                    id = 1,
                    name = "Colombo US Embassy",
                    latitude = 6.913047,
                    longitude = 79.84807,
                    aqi = 82,
                    pm25 = 82,
                    pm10 = 40,
                    temp = 35,
                    humidity = 49,
                    pressure = 1011,
                    wind = 8.2
                },
                new {
                    id = 2,
                    name = "Another Location",
                    latitude = 6.9000,
                    longitude = 79.8600,
                    aqi = 95,
                    pm25 = 90,
                    pm10 = 50,
                    temp = 34,
                    humidity = 55,
                    pressure = 1012,
                    wind = 6.5
                }
            }
        };

        return Ok(data);
    }
}
