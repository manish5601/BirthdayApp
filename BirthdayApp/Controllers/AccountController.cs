using Microsoft.AspNetCore.Mvc;
using BirthdayApp.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BirthdayApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly BirthdayContext _context;
        public AccountController(BirthdayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string UserEmail, string UserPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserEmail == UserEmail && u.UserPassword == UserPassword);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim("DisplayName", user.UserName),
                    new Claim(ClaimTypes.Email, user.UserEmail)
                };
                var claimsIdentity = new ClaimsIdentity(claims, "MyAuth");
                await HttpContext.SignInAsync("MyAuth", new ClaimsPrincipal(claimsIdentity));
                return RedirectToAction("Index", "UserList");
            }
            ViewBag.Error = "Invalid email or password";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string UserName, string UserEmail, string UserPassword, DateTime DateOfBirth)
        {
            if (_context.Users.Any(u => u.UserEmail == UserEmail))
            {
                ViewBag.Error = "Email already registered.";
                return View();
            }
            var user = new UserList
            {
                UserName = UserName,
                UserEmail = UserEmail,
                UserPassword = UserPassword,
                DateOfBirth = DateOnly.FromDateTime(DateOfBirth)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }
    }
} 