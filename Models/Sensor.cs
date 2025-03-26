using Microsoft.AspNetCore.Identity;

namespace MetroAir.Models
{
    public class Sensor
    {
        public int Id { get; set; }  // Assuming this is the primary key
        public string LocationName { get; set; }
        public string Type { get; set; }
        public int StationId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int AQI { get; internal set; }
        public string AirQualityStatus { get; set; } = "Default"; 
    }



}
