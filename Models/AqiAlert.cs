// Models/AqiAlert.cs

namespace MetroAir.Models
{
    public class AqiAlert
    {
        public int Id { get; set; }
        public int SensorId { get; set; }
        public Sensor Sensor { get; set; }
        public int AqiValue { get; set; }
        public string AlertLevel { get; set; } // "Moderate", "Unhealthy", etc.
        public DateTime Timestamp { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // Models/AlertNotification.cs
    public class AlertNotification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string AlertType { get; set; } // "info", "warning", "danger", etc.
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public string CreatedForUserId { get; set; } // If you want user-specific alerts
    }
}