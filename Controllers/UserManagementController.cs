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
            var usersList = await _userManager.Users.ToListAsync(); // Fetch all users

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


        [HttpPost]
        public async Task<IActionResult> AddUser(string FullName, string Email, string Role, string Password, string ConfirmPassword)
        {
            // Check if all required fields are filled
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Role) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["Error"] = "All fields are required!";
                return RedirectToAction("UserManagement");
            }

            // Check if password and confirm password match
            if (Password != ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match!";
                return RedirectToAction("UserManagement");
            }

            // Create a new user
            var user = new Users
            {
                FullName = FullName,
                Email = Email,
                UserName = Email
            };

          
            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                // Assign the user to the selected role
                await _userManager.AddToRoleAsync(user, Role);
                TempData["Success"] = "User added successfully!";
            }
            else
            {
                TempData["Error"] = "User creation failed! " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("UserManagement");
        }

    }
    }