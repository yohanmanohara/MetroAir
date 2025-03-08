using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MetroAir.Models; // Namespace for User model
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;

namespace MetroAir.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Signup
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        // POST: /Account/Signup
        // POST: /Account/Signup
[HttpPost]
public async Task<IActionResult> SignUp(User user)
{
    // Mark that the form was submitted
    TempData["FormSubmitted"] = true;

    var existingUser = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == user.Email);

    if (existingUser != null)
    {
        TempData["Error"] = "Email is already in use.";
        return View(user); 
    }

    user.PasswordHash = HashPassword(user.PasswordHash);

    try
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Sign-up successful! Please log in.";
        
       return View(user); 

    }
    catch (Exception)
    {
        TempData["Error"] = "An error occurred while signing up. Please try again.";
        return View(user);
    }
}

        // Hash password method using BCrypt
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(); // Return to login page with error message
            }

            // Store user session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Dashboard", "Home");
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session data
            return RedirectToAction("Login");
        }
    }
}
