using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserRoles.Models;
using UserRoles.ViewModels; 

namespace UserRoles.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET:
        [Route("Home/UserManagement")]
        public async Task<IActionResult> UserManagement()
        {
            // List of emails to exclude
            var excludedEmails = new List<string>
    {
        "admin@gmail.com",
        "dataprovider@gmail.com",
        "monitoringadmin@gmail.com"
    };

            // Fetch all users except those with excluded emails
            var usersList = await _userManager.Users
                .Where(u => !excludedEmails.Contains(u.Email))
                .ToListAsync();

            var users = new List<UserViewModel>();

            foreach (var u in usersList)
            {
                var roles = await _userManager.GetRolesAsync(u);
                users.Add(new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Roles = roles
                });
            }

            return View("~/Views/Home/WebMaster/UserManagement.cshtml", users);
        }


        // POST: UserManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to delete user.");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}