using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MetroAir.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Linq;
using UserRoles.Data;

namespace MetroAir.Services
{
    public class AirQualityBackgroundService : BackgroundService
    {
        private const string API_KEY = "afd0a6ef4a6ae0b9fd8a41474b30a4910fbe3b3e";
        private readonly IServiceProvider _services;
        private readonly ILogger<AirQualityBackgroundService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        // Remove hardcoded defaults - will use database values
        private TimeSpan _updateInterval;
        private bool _isRunning;
        private SimulationSettings _settings;

        public AirQualityBackgroundService(
            IServiceProvider services,
            ILogger<AirQualityBackgroundService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _services = services;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Air Quality Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Load and apply settings
                    await UpdateServiceSettings(dbContext);

                    _logger.LogDebug($"Service state - IsRunning: {_isRunning}, Interval: {_updateInterval.TotalMinutes} mins");

                    if (!_isRunning)
                    {
                        _logger.LogDebug("Service paused - waiting 1s");
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    var startTime = DateTime.UtcNow;
                    await ProcessActiveSensorsAsync(dbContext, stoppingToken);

                    var elapsed = DateTime.UtcNow - startTime;
                    var delay = _updateInterval - elapsed;

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    _logger.LogError(ex, "Error in background service");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            _logger.LogInformation("Air Quality Background Service stopped");
        }

        private async Task UpdateServiceSettings(AppDbContext dbContext)
        {
            _settings = await dbContext.SimulationSettings.FirstOrDefaultAsync();

            if (_settings == null)
            {
                _logger.LogWarning("No simulation settings found - using defaults");
                _settings = new SimulationSettings(); // Use model defaults
            }

            _updateInterval = TimeSpan.FromMinutes(_settings.UpdateFrequencyMinutes);
            _isRunning = _settings.IsRunning;

            _logger.LogInformation($"Settings updated - Running: {_isRunning}, Interval: {_updateInterval.TotalMinutes} mins");
        }

        private async Task ProcessActiveSensorsAsync(AppDbContext dbContext, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Starting sensor data processing...");

                if (!await dbContext.Database.CanConnectAsync(ct))
                {
                    _logger.LogError("Cannot connect to database");
                    return;
                }

                var activeSensors = await dbContext.Sensors
                    .Where(s => s.Status == "Active")
                    .ToListAsync(ct);

                if (!activeSensors.Any())
                {
                    _logger.LogInformation("No active sensors found");
                    return;
                }

                await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

                try
                {
                    foreach (var sensor in activeSensors)
                    {
                        await ProcessSingleSensor(dbContext, sensor, ct);
                    }

                    var changes = await dbContext.SaveChangesAsync(ct);
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Saved {changes} changes ({activeSensors.Count} sensors updated)");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Transaction failed. Changes rolled back.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process sensors");
            }
        }

        private async Task ProcessSingleSensor(AppDbContext dbContext, Sensor sensor, CancellationToken ct)
        {
            try
            {
                _logger.LogDebug($"Processing sensor {sensor.Id} at {sensor.LocationName}");

                var (aqi, status, pollutants) = await GetRealTimeAirQualityData(sensor.StationId);

                // Update sensor
                sensor.AQI = aqi;
                sensor.AirQualityStatus = status;
                sensor.LastUpdated = DateTime.UtcNow;

                // Add history
                dbContext.AirQualityHistory.Add(new AirQualityHistory
                {
                    SensorId = sensor.Id,
                    Timestamp = DateTime.UtcNow,
                    AQI = aqi,
                    Status = status,
                    PM2_5 = pollutants?.PM2_5 ?? 0,
                    PM10 = pollutants?.PM10 ?? 0,
                    NO2 = pollutants?.NO2 ?? 0,
                    SO2 = pollutants?.SO2 ?? 0,
                    CO = pollutants?.CO ?? 0,
                    O3 = pollutants?.O3 ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing sensor {sensor.Id}");
            }
        }

        private async Task<(int AQI, string Status, PollutantValues Pollutants)> GetRealTimeAirQualityData(int stationId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://api.waqi.info/feed/@{stationId}/?token={API_KEY}";

                _logger.LogDebug($"Fetching air quality data for station {stationId}");
                var response = await client.GetStringAsync(url);
                var data = JObject.Parse(response);

                if (data["status"]?.ToString() != "ok")
                {
                    _logger.LogWarning($"API returned non-ok status: {data["status"]}");
                    return (0, "Unknown", null);
                }

                // Debug log the full response structure
                _logger.LogTrace($"Full API response: {data.ToString()}");

                var aqi = data["data"]?["aqi"]?.ToObject<int>() ?? 0;
                var pollutants = ExtractPollutants(data["data"]?["iaqi"]);

                if (pollutants != null)
                {
                    _logger.LogInformation($"Extracted pollutants for station {stationId}: " +
                                         $"PM2.5={pollutants.PM2_5}, PM10={pollutants.PM10}, " +
                                         $"NO2={pollutants.NO2}, SO2={pollutants.SO2}, " +
                                         $"CO={pollutants.CO}, O3={pollutants.O3}");
                }

                return (aqi, GetAQIStatus(aqi), pollutants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching air quality data for station {stationId}");
                return (0, "Unknown", null);
            }
        }

        private PollutantValues ExtractPollutants(JToken iaqiData)
        {
            if (iaqiData == null)
            {
                _logger.LogWarning("No iaqi data found in API response");
                return null;
            }

            var pollutants = new PollutantValues();
            bool hasData = false;

            // Handle all possible pollutant name variations
            var pollutantMappings = new Dictionary<string, Action<double>>(StringComparer.OrdinalIgnoreCase)
            {
                ["pm25"] = v => { pollutants.PM2_5 = v; hasData = true; },
                ["pm2.5"] = v => { pollutants.PM2_5 = v; hasData = true; },
                ["pm10"] = v => { pollutants.PM10 = v; hasData = true; },
                ["no2"] = v => { pollutants.NO2 = v; hasData = true; },
                ["so2"] = v => { pollutants.SO2 = v; hasData = true; },
                ["co"] = v => { pollutants.CO = v; hasData = true; },
                ["o3"] = v => { pollutants.O3 = v; hasData = true; }
            };

            foreach (var item in iaqiData)
            {
                if (item is JProperty property)
                {
                    try
                    {
                        var lowerName = property.Name.ToLower();
                        if (pollutantMappings.TryGetValue(lowerName, out var setter))
                        {
                            var valueToken = property.Value["v"];
                            if (valueToken != null)
                            {
                                var value = valueToken.Value<double>();
                                setter(value);
                                _logger.LogDebug($"Found pollutant {property.Name}: {value}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error processing pollutant {property.Name}: {ex.Message}");
                    }
                }
            }

            if (!hasData)
            {
                _logger.LogWarning("No valid pollutant data found in iaqi section");
                return null;
            }

            return pollutants;
        }
        private double GetPollutantValue(JToken iaqiData, string pollutantName) =>
            iaqiData[pollutantName]?["v"]?.ToObject<double>() ?? 0;

        private string GetAQIStatus(int aqi) => aqi switch
        {
            <= 50 => "Good",
            <= 100 => "Fair",
            <= 150 => "Moderate",
            <= 200 => "Poor",
            <= 300 => "Very Poor",
            _ => "Hazardous"
        };
    }

    public class PollutantValues
    {
        public double PM2_5 { get; set; }
        public double PM10 { get; set; }
        public double NO2 { get; set; }
        public double SO2 { get; set; }
        public double CO { get; set; }
        public double O3 { get; set; }
    }
}