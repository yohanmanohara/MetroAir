using MetroAir.Models;

namespace MetroAir.ViewModels
{
    internal class SystemStatusViewModel
    {
        public List<Sensor> Sensors { get; set; }
        public int ActiveSensorsCount { get; set; }
        public int TotalSensorsCount { get; set; }
        public bool IsRunning { get; set; }
        public int UpdateIntervalMinutes { get; set; }
    }
}