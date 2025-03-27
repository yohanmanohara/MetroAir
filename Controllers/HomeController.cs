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
        public IActionResult SystemConfiguration()
        {
            return View("~/Views/Home/WebMaster/SystemConfiguration.cshtml");
        }


       
        [Authorize(Roles = "WebMaster")]
        public IActionResult Basicsecuritysetup()
        {
            return View("~/Views/Home/WebMaster/Basicsecuritysetup.cshtml");
        }

        [Authorize(Roles = "WebMaster")]
        public IActionResult UserManagement()
        {
            return View("~/Views/Home/WebMaster/UserManagement.cshtml");
        }




        //data provider
        [Authorize(Roles = "DataProvider")]
        public IActionResult SystemLog()
        {
            return View("~/Views/Home/DataProvider/SystemLog.cshtml"); ;
        }
        [Authorize(Roles = "DataProvider")]
        public IActionResult SystemReport()
        {
            return View("~/Views/Home/DataProvider/SystemReport.cshtml"); ;
        }
       

        //Monitoring admin



        [Authorize(Roles = "MonitoringAdmin")]
        public IActionResult Overview()
        {
            return View("~/Views/Home/Monitoringadmin/overview.cshtml");
        }

        [Authorize(Roles = "MonitoringAdmin")]
        public IActionResult SensorManagement()
        {
            return View("~/Views/Home/Monitoringadmin/SensorManagement.cshtml");
        }
        [Authorize(Roles = "MonitoringAdmin")]
        public IActionResult MonitoringUserManagement()
        {
            return View("~/Views/Home/Monitoringadmin/MonitoringUserManagement.cshtml");
        }
        [Authorize(Roles = "MonitoringAdmin")]
        public IActionResult DataSimulationManagement()
        {
            return View("~/Views/Home/Monitoringadmin/datasimulationmanagement.cshtml");
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
