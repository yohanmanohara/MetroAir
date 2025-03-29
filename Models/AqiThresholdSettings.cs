    // Models/AqiThresholdSettings.cs
    using System.ComponentModel.DataAnnotations;
    namespace MetroAir.Models
    {
        public class AqiThresholdSettings
        {
            public int Id { get; set; } = 1; // Single record in DB

            [Range(0, 1000)]
            public int GoodThreshold { get; set; } = 50;

            [Range(0, 1000)]
            public int ModerateThreshold { get; set; } = 100;

            [Range(0, 1000)]
            public int UnhealthySensitiveThreshold { get; set; } = 150;

            [Range(0, 1000)]
            public int UnhealthyThreshold { get; set; } = 200;

            [Range(0, 1000)]
            public int VeryUnhealthyThreshold { get; set; } = 300;

            [Range(0, 1000)]
            public int HazardousThreshold { get; set; } = 500;
        }
    }