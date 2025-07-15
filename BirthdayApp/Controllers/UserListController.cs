using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BirthdayApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using BirthdayApp.Services;

namespace BirthdayApp.Controllers
{
    [Authorize]
    public class UserListController : Controller
    {
        private readonly BirthdayContext _context;
        private readonly IEmailService _emailService;

        public UserListController(BirthdayContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Landing()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                UserList? user = _context.Users.Where(u => u.UserEmail.Equals(login.UserEmail) && u.UserPassword.Equals(login.UserPassword)).FirstOrDefault();
                if (user != null)
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                        new Claim("DisplayName", user.UserName),
                        new Claim(ClaimTypes.Email, user.UserEmail)
                    };
                    
                    var claimsIdentity = new ClaimsIdentity(claims, "MyAuth");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    
                    await HttpContext.SignInAsync("MyAuth", claimsPrincipal);
                    return RedirectToAction("Index");
                }
            }
            ModelState.AddModelError("", "Login failed.");
            return View(login);
        }
        [HttpGet]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync("MyAuth");
            return RedirectToAction(nameof(Login));
        }
        // GET: UserList
        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Landing");
            }
            return View(await _context.Users.ToListAsync());
        }

        // GET: UserList/FilterByBirthday
        public async Task<IActionResult> FilterByBirthday(int? month, int? day)
        {
            var filterModel = new BirthdayFilterModel();
            
            // If no month/day provided, show today's birthdays
            if (!month.HasValue || !day.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                month = today.Month;
                day = today.Day;
                filterModel.SelectedDate = today;
            }
            else
            {
                // Create a date for the selected month and day (using current year)
                var currentYear = DateTime.Today.Year;
                filterModel.SelectedDate = new DateOnly(currentYear, month.Value, day.Value);
            }

            // Get users with the same birthday (month and day)
            filterModel.UsersWithSameBirthday = await _context.Users
                .Where(u => u.DateOfBirth.Month == month.Value && 
                           u.DateOfBirth.Day == day.Value)
                .ToListAsync();

            // Get birthday wishes for this date
            filterModel.BirthdayWishes = await _context.BirthdayWishes
                .Where(w => w.BirthdayDate.Month == month.Value && 
                           w.BirthdayDate.Day == day.Value)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(filterModel);
        }

        // GET: UserList/SendWish
        public async Task<IActionResult> SendWish(int toUserId, DateOnly birthdayDate)
        {
            var receiver = await _context.Users.FindAsync(toUserId);
            if (receiver == null)
            {
                return NotFound();
            }

            var wishModel = new BirthdayWishModel
            {
                ToUserId = toUserId,
                ToUserName = receiver.UserName,
                BirthdayDate = birthdayDate
            };

            return View(wishModel);
        }

        // POST: UserList/SendWish
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendWish(BirthdayWishModel wishModel)
        {
            if (ModelState.IsValid)
            {
                // Get current user ID from claims
                var currentUserIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    // If no authenticated user, redirect to login
                    return RedirectToAction("Login");
                }

                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                {
                    // If user not found in database, redirect to login
                    return RedirectToAction("Login");
                }

                var receiver = await _context.Users.FindAsync(wishModel.ToUserId);
                if (receiver == null)
                {
                    return NotFound();
                }

                var birthdayWish = new BirthdayWish
                {
                    WishMessage = wishModel.WishMessage,
                    FromUserId = currentUserId,
                    FromUserName = currentUser.UserName,
                    ToUserId = wishModel.ToUserId,
                    ToUserName = wishModel.ToUserName,
                    BirthdayDate = wishModel.BirthdayDate,
                    CreatedAt = DateTime.Now,
                    EmailSent = false
                };

                _context.BirthdayWishes.Add(birthdayWish);
                await _context.SaveChangesAsync();

                // Send email notification
                var emailModel = new EmailNotificationModel
                {
                    ToEmail = receiver.UserEmail,
                    ToName = receiver.UserName,
                    FromName = currentUser.UserName,
                    WishMessage = wishModel.WishMessage,
                    BirthdayDate = wishModel.BirthdayDate
                };

                var (emailSent, emailError) = await _emailService.SendBirthdayWishEmailAsync(emailModel);
                
                // Update the wish record with email status
                birthdayWish.EmailSent = emailSent;
                await _context.SaveChangesAsync();

                if (emailSent)
                {
                    TempData["SuccessMessage"] = $"Birthday wish sent to {wishModel.ToUserName}! Email notification sent.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Birthday wish sent to {wishModel.ToUserName}! Email notification failed: {emailError}";
                }
                return RedirectToAction(nameof(FilterByBirthday), new { month = wishModel.BirthdayDate.Month, day = wishModel.BirthdayDate.Day });
            }

            return View(wishModel);
        }

        // GET: UserList/ViewWishes
        public async Task<IActionResult> ViewWishes(DateOnly? selectedDate)
        {
            if (!selectedDate.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            var wishes = await _context.BirthdayWishes
                .Where(w => w.BirthdayDate.Month == selectedDate.Value.Month && 
                           w.BirthdayDate.Day == selectedDate.Value.Day)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            ViewBag.SelectedDate = selectedDate.Value;
            return View(wishes);
        }

        // GET: UserList/MyWishes
        public async Task<IActionResult> MyWishes()
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
                return RedirectToAction("Login");

            var wishes = await _context.BirthdayWishes
                .Where(w => w.ToUserId == currentUserId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(wishes);
        }

        // GET: UserList/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userList = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userList == null)
            {
                return NotFound();
            }

            return View(userList);
        }

        // GET: UserList/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserList/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserName,UserEmail,UserPassword,DateOfBirth")] UserList userList)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userList);
        }

        // GET: UserList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userList = await _context.Users.FindAsync(id);
            if (userList == null)
            {
                return NotFound();
            }
            return View(userList);
        }

        // POST: UserList/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,UserEmail,UserPassword,DateOfBirth")] UserList userList)
        {
            if (id != userList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserListExists(userList.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(userList);
        }

        // GET: UserList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userList = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userList == null)
            {
                return NotFound();
            }

            return View(userList);
        }

        // POST: UserList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userList = await _context.Users.FindAsync(id);
            if (userList != null)
            {
                _context.Users.Remove(userList);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserListExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // Helper method to check if today is user's birthday
        private bool IsTodayBirthday(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return dateOfBirth.Month == today.Month && dateOfBirth.Day == today.Day;
        }
    }
}
