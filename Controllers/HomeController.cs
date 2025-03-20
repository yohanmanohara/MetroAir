using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

       
        [Authorize(Roles = "WebMaster")]
        public IActionResult WebMaster()
        {
            return View();
        }
        [Authorize(Roles = "DataProvider")]
        public IActionResult DataProvider()
        {
            return View();
        }
        [Authorize(Roles = "MonitoringAdmin")]
        public IActionResult MonitoringAdmin()
        {
            return View();
        }

        //[Authorize(Roles = "User")]
        //public IActionResult User()
        //{
        //    return View();
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
