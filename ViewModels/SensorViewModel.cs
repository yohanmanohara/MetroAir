using System;
using System.ComponentModel.DataAnnotations;

namespace MetroAir.ViewModels
{
    public class SensorViewModel
    {
        [Required(ErrorMessage = "Location name is required")]
        public string LocationName { get; set; }

        [Required(ErrorMessage = "Sensor type is required")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Station ID is required")]
        public int StationId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
