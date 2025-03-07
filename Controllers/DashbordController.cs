    using Microsoft.AspNetCore.Mvc;

    namespace MetroAir.Controllers
    {
        [Route("Dashboard")]
        public class DashboardController : Controller
        {
            [HttpGet("user")]
            public IActionResult UserDashboard()
            {
                return View("User/Dashboard"); // Specify the subfolder correctly
            }
        }
    }
