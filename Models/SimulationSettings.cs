namespace MetroAir.Models
{
    public class SimulationSettings
    {
        public int Id { get; set; } = 1;
        public int UpdateFrequencyMinutes { get; set; } = 5;
        public bool IsRunning { get; set; } = true;
        public double BasePM2_5 { get; set; } = 30.0;
        public double BasePM10 { get; set; } = 45.0;
        public double BaseNO2 { get; set; } = 15.0;
        public double BaseSO2 { get; set; } = 5.0;
        public double BaseCO { get; set; } = 0.5;
        public double BaseO3 { get; set; } = 20.0;
        public double DailyVariationFactor { get; set; } = 0.3;
        public double RandomVariationFactor { get; set; } = 0.2;
    }
}