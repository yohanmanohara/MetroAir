using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

public class SensorController : Controller
{
    private static readonly string API_KEY = "afd0a6ef4a6ae0b9fd8a41474b30a4910fbe3b3e"; 
    public async Task<ActionResult> Index()
    {
        ViewBag.Sensors = await GetSensorsWithAQI();
        return View();
    }

    public async Task<List<Sensor>> GetSensorsWithAQI()
    {
        var sensors = new List<Sensor>
        {
            new Sensor { LocationName = "Central Park", XCoordinate = 40.785091, YCoordinate = -73.968285, Status = "Active", CreatedDate = DateTime.Now },
            new Sensor { LocationName = "Times Square", XCoordinate = 40.758896, YCoordinate = -73.985130, Status = "Active", CreatedDate = DateTime.Now }
        };

        foreach (var sensor in sensors)
        {
            var airQualityData = await GetAQIData(sensor.XCoordinate, sensor.YCoordinate);
            if (airQualityData != null)
            {
                sensor.AQI = airQualityData.Item1;
                sensor.AirQualityStatus = airQualityData.Item2;
            }
        }

        return sensors;
    }

    private async Task<Tuple<int, string>> GetAQIData(double lat, double lon)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://api.waqi.info/feed/geo:{lat};{lon}/?token={API_KEY}";

            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(json);

                if (data["status"].ToString() == "ok")
                {
                    int aqi = (int)data["data"]["aqi"];
                    string status = GetAQIStatus(aqi);
                    return Tuple.Create(aqi, status);
                }
            }
        }
        return null;
    }

    private string GetAQIStatus(int aqi)
    {
        if (aqi <= 50) return "Good";
        if (aqi <= 100) return "Moderate";
        if (aqi <= 150) return "Unhealthy for Sensitive Groups";
        if (aqi <= 200) return "Unhealthy";
        if (aqi <= 300) return "Very Unhealthy";
        return "Hazardous";
    }
}
