    // Models/AirQualityHistory.cs
    using MetroAir.Models;

    public class AirQualityHistory
    {
        public int Id { get; set; }
        public int SensorId { get; set; } // Foreign key to Sensor
        public Sensor Sensor { get; set; } // Navigation property
        public DateTime Timestamp { get; set; }
        public int AQI { get; set; }
        public string Status { get; set; }
        public double PM2_5 { get; set; }
        public double PM10 { get; set; }
        public double NO2 { get; set; }
        public double SO2 { get; set; }
        public double CO { get; set; }
        public double O3 { get; set; }
    }