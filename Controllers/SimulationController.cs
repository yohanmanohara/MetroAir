using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetroAir.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using UserRoles.Data;
using System.Threading.Tasks;
using MetroAir.Services;

namespace MetroAir.Controllers
{
    [Authorize(Roles = "MonitoringAdmin")]
    public class SimulationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public SimulationController(
            AppDbContext context,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        [Route("Home/DataSimulationManagement")]
        public async Task<IActionResult> DataSimulationManagement()
        {
            try
            {
                var settings = await _context.SimulationSettings.FirstOrDefaultAsync();
                if (settings == null)
                {
                    settings = new SimulationSettings
                    {
                        Id = 1,
                        UpdateFrequencyMinutes = 9,
                        IsRunning = false,
                        BasePM2_5 = 30.0,
                        BasePM10 = 45.0,
                        BaseNO2 = 15.0,
                        BaseSO2 = 5.0,
                        BaseCO = 0.5,
                        BaseO3 = 20.0,
                        DailyVariationFactor = 0.3,
                        RandomVariationFactor = 0.2
                    };
                    _context.SimulationSettings.Add(settings);
                    await _context.SaveChangesAsync();
                }
                return View("~/Views/Home/Monitoringadmin/DataSimulationManagement.cshtml", settings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to load simulation settings: " + ex.Message;
                return View("~/Views/Home/Monitoringadmin/DataSimulationManagement.cshtml", new SimulationSettings());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Home/DataSimulationManagement/UpdateSettings")]
        public async Task<IActionResult> UpdateSettings(SimulationSettings model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the form errors";
                return View("~/Views/Home/Monitoringadmin/DataSimulationManagement.cshtml", model);
            }

            try
            {
                var settings = await _context.SimulationSettings.FirstOrDefaultAsync();
                if (settings == null)
                {
                    settings = new SimulationSettings { Id = 1 };
                    _context.SimulationSettings.Add(settings);
                }

                settings.UpdateFrequencyMinutes = model.UpdateFrequencyMinutes;
                settings.BasePM2_5 = model.BasePM2_5;
                settings.BasePM10 = model.BasePM10;
                settings.BaseNO2 = model.BaseNO2;
                settings.BaseSO2 = model.BaseSO2;
                settings.BaseCO = model.BaseCO;
                settings.BaseO3 = model.BaseO3;
                settings.DailyVariationFactor = model.DailyVariationFactor;
                settings.RandomVariationFactor = model.RandomVariationFactor;
                settings.IsRunning = model.IsRunning;

                await _context.SaveChangesAsync();

                using var scope = _serviceProvider.CreateScope();
                var backgroundService = scope.ServiceProvider.GetService<AirQualityBackgroundService>();

                TempData["SuccessMessage"] = "Settings updated successfully";
                return RedirectToAction("DataSimulationManagement");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update settings: " + ex.Message;
                return View("~/Views/Home/Monitoringadmin/DataSimulationManagement.cshtml", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Home/DataSimulationManagement/ToggleSimulation")]
        public async Task<IActionResult> ToggleSimulation([FromBody] bool isRunning)
        {
            try
            {
                var settings = await _context.SimulationSettings.FirstOrDefaultAsync();
                if (settings == null)
                {
                    settings = new SimulationSettings { Id = 1, IsRunning = isRunning };
                    _context.SimulationSettings.Add(settings);
                }
                else
                {
                    settings.IsRunning = isRunning;
                }

                await _context.SaveChangesAsync();

                using var scope = _serviceProvider.CreateScope();
                var backgroundService = scope.ServiceProvider.GetService<AirQualityBackgroundService>();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}