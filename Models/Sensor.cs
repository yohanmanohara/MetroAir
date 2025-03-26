public class Sensor
{
    public string LocationName { get; set; }
    public string Type { get; set; }
    public double XCoordinate { get; set; }
    public double YCoordinate { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public int AQI { get; set; } // Air Quality Index
    public string AirQualityStatus { get; set; } // Good, Moderate, Unhealthy
}
