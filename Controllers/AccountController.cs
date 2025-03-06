using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Display Signup form
    [HttpGet]
    public IActionResult Signup()
    {
        return View();
    }

    // Handle Signup form submission
    [HttpPost]
    public async Task<IActionResult> Signup(string username, string email, string password)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Username == username);
        if (userExists)
        {
            return BadRequest("User already exists");
        }

        // Hash the password
        using var sha256 = SHA256.Create();
        var passwordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Redirect to Login page after successful sign-up
        return RedirectToAction("Login");
    }

    // Display Login form
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Handle Login form submission
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            return BadRequest("User not found");
        }

        using var sha256 = SHA256.Create();
        var passwordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));

        if (user.PasswordHash != passwordHash)
        {
            return BadRequest("Invalid password");
        }

        // Redirect to Home or Dashboard page after successful login
        return RedirectToAction("Index", "Home");
    }
}
