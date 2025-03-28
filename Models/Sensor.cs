using Microsoft.AspNetCore.Identity;

namespace MetroAir.Models
{
    // Models/Sensor.cs
    public class Sensor
    {
        public int Id { get; set; }
        public string LocationName { get; set; }
        public string Type { get; set; }
        public int StationId { get; set; }
        public string Status { get; set; } = "Inactive"; // Default to inactive
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; }
        public int AQI { get; set; }
        public string AirQualityStatus { get; set; } = "Unknown";

        // Navigation property for historical data
        public ICollection<AirQualityHistory> HistoricalData { get; set; }

    }

}   



