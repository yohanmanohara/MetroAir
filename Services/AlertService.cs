// Services/AlertService.cs
using MetroAir.Models;
using UserRoles.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
public class AlertService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AlertService> _logger;

    public AlertService(AppDbContext context, ILogger<AlertService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CheckAndCreateAlerts(IEnumerable<Sensor> sensors)
    {
        try
        {
            var thresholds = await _context.AqiThresholdSettings.FirstOrDefaultAsync();
            if (thresholds == null)
            {
                _logger.LogWarning("No AQI threshold settings found in database");
                return;
            }

            foreach (var sensor in sensors)
            {
                // Skip sensors without AQI data
                if (sensor.AQI == null)
                {
                    continue;
                }

                var aqi = sensor.AQI.Value;
                string alertLevel = DetermineAlertLevel(aqi, thresholds);

                if (alertLevel != null)
                {
                    await ProcessActiveAlert(sensor, alertLevel, aqi);
                }
                else
                {
                    await ResolveExistingAlerts(sensor.Id);
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking and creating alerts");
            throw; // Rethrow to allow caller to handle
        }
    }

    private string DetermineAlertLevel(int aqi, AqiThresholdSettings thresholds)
    {
        if (aqi > thresholds.VeryUnhealthyThreshold)
            return "Hazardous";
        if (aqi > thresholds.UnhealthyThreshold)
            return "Very Unhealthy";
        if (aqi > thresholds.UnhealthySensitiveThreshold)
            return "Unhealthy";
        if (aqi > thresholds.ModerateThreshold)
            return "Unhealthy for Sensitive Groups";

        return null;
    }

    private async Task ProcessActiveAlert(Sensor sensor, string alertLevel, int aqi)
    {
        var existingAlert = await _context.AqiAlerts
            .FirstOrDefaultAsync(a => a.SensorId == sensor.Id && a.IsActive);

        if (existingAlert == null)
        {
            // Create new alert
            var alert = new AqiAlert
            {
                SensorId = sensor.Id,
                AqiValue = aqi,
                AlertLevel = alertLevel,
                Timestamp = DateTime.UtcNow
            };

            _context.AqiAlerts.Add(alert);
            await CreateNotification(sensor, alertLevel, aqi);
        }
        else if (existingAlert.AlertLevel != alertLevel)
        {
            // Update existing alert if level changed
            existingAlert.AlertLevel = alertLevel;
            existingAlert.AqiValue = aqi;
            existingAlert.Timestamp = DateTime.UtcNow;
            await CreateNotification(sensor, alertLevel, aqi);
        }
    }

    private async Task ResolveExistingAlerts(int sensorId)
    {
        var existingAlerts = await _context.AqiAlerts
            .Where(a => a.SensorId == sensorId && a.IsActive)
            .ToListAsync();

        foreach (var alert in existingAlerts)
        {
            alert.IsActive = false;
        }
    }

    private async Task CreateNotification(Sensor sensor, string alertLevel, int aqi)
    {
        var notification = new AlertNotification
        {
            Message = $"{alertLevel} air quality detected at {sensor.LocationName} (AQI: {aqi})",
            AlertType = GetAlertType(alertLevel),
            CreatedAt = DateTime.UtcNow,
            CreatedForUserId = null
        };

        _context.AlertNotifications.Add(notification);
    }

    private string GetAlertType(string alertLevel)
    {
        return alertLevel switch
        {
            "Hazardous" => "dark",
            "Very Unhealthy" => "danger",
            "Unhealthy" => "warning",
            "Unhealthy for Sensitive Groups" => "info",
            _ => "primary" // Default case
        };
    }
}