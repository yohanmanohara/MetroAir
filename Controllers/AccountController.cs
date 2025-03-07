using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MetroAir.Utils; // Namespace for PasswordHasher
using MetroAir.Models; // Namespace for User model
using Microsoft.EntityFrameworkCore;

namespace MetroAir.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(User user)
        {
            if (ModelState.IsValid)
            {
                user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                // Authentication successful
                return RedirectToAction("Dashboard");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }
    }
}
