using BCrypt.Net;
using DCAS.Data;
using DCAS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DCAS.Controllers
{
    public class LoginController : Controller
    {
        private readonly DCASContext _context;

        public LoginController(DCASContext context)
        {
            _context = context;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            // Remove "0x" prefix if it exists
            hex = hex.StartsWith("0x") ? hex.Substring(2) : hex;

            // Check if the string has an even number of characters
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have an even number of characters.");
            }

            byte[] byteArray = new byte[hex.Length / 2];
            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return byteArray;
        }
        public async Task<IActionResult> Index()
        {

            if (!await _context.Users.AnyAsync(u => u.Username == "admin"))
            {
                var hexString = "0x524946462E0C00005745425056503820220C00003056009D012ADA010A013E9D4EA34D25A4A7A5A252B908F01389696EE174A10FB52854FCF65D0234335BCF05EC09FDA3FE1FB01FF32FF05EB1FFEC7997297E75A89678B6AB2DE00933F40AD6FB82F705EE0BDC17B82F6FC6B07C6E3B17C67A5045042F3D8ACC565EAC64EBCE";
                byte[] profileData = HexStringToByteArray(hexString);

                var adminUser = new Users
                {
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 17),
                    Profile = profileData, // Assign the converted byte array
                    Name = "Administrator",
                    Role = "Admin",
                    Email = "admin@example.com",
                    PhoneNumber = "123-456-7890",
                    JobTitle = "System Administrator",
                    Specialization = "Skilling",
                    Address = "123 Admin St.",
                    Gender = "Male",
                    Nationality = "Filipino",
                    Position = "Administrator",
                    WorkStatus = "Active",
                    Age = "40",
                    BirthDate = DateTime.Parse("1985-01-01"),
                    StartDate = DateTime.Now,
                };
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }

            return View();
        }

        // Updated Login Action with BCrypt for password verification
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Retrieve user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password)) // Password is hashed in DB, verify using BCrypt
            {
                // Generate claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UsersId", user.UsersId.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // This will keep the user logged in even after browser closes
                };

                // Sign in the user
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Role-based redirection
                if (user.Role == "Admin" || user.Role == "Staff")
                {
                    return RedirectToAction("Index", "Home");
                }

                // Default redirection for other roles (if any)
                return RedirectToAction("Index", "Home");
            }

            // If login fails, show error
            TempData["Error"] = "Invalid username or password.";
            return View("Index");
        }

        // Logout Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login"); // Redirect to login page
        }
    }
}
