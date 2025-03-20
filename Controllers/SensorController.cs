using Microsoft.AspNetCore.Mvc;

namespace MetroAir.Controllers
{
    public class SensorController : Controller
    {
        public IActionResult Sensor()
        {
            return View();
        }
    }
}
