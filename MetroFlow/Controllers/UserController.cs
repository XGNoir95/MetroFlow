using MetroFlow.Models;
using MetroFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MetroFlow.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MailService _mail;

        public UserController(ApplicationDbContext context, MailService mail)
        {
            _context = context;
            _mail = mail;
        }

        // ----------------- SIGNUP -----------------
        [HttpGet]
        public IActionResult Signup()
        {
            ViewBag.Users = _context.Users.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid data. Please check your inputs.";
                ViewBag.Users = await _context.Users.ToListAsync();
                return View(user);
            }

            var exists = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (exists)
            {
                ViewBag.Message = "Email already registered!";
                ViewBag.Users = await _context.Users.ToListAsync();
                return View(user);
            }

            user.CreatedAt = DateTime.UtcNow;

            // Generate a 6-digit OTP
            var rnd = new Random();
            user.Otp = rnd.Next(100000, 999999).ToString();
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);
            user.IsVerified = false;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send OTP
            _mail.SendOtp(user.Email, user.Otp!);

            TempData["Flash"] = "We emailed you an OTP. Please verify.";
            return RedirectToAction("VerifyOtp", new { email = user.Email });
        }

        // ----------------- VERIFY OTP -----------------
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Flash"] = "Missing email. Please signup again.";
                return RedirectToAction("Signup");
            }
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
            {
                ViewBag.Message = "Email and OTP are required.";
                ViewBag.Email = email;
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "User not found.";
                ViewBag.Email = email;
                return View();
            }

            if (user.IsVerified)
            {
                TempData["Flash"] = "Your account is already verified. Please login.";
                return RedirectToAction("Login");
            }

            if (user.Otp == otp && user.OtpExpiry.HasValue && user.OtpExpiry > DateTime.UtcNow)
            {
                user.IsVerified = true;
                user.Otp = null;
                user.OtpExpiry = null;
                await _context.SaveChangesAsync();

                TempData["Flash"] = "Account verified! Please login.";
                return RedirectToAction("Login");
            }

            ViewBag.Message = "Invalid or expired OTP.";
            ViewBag.Email = email;
            return View();
        }

        // ----------------- RESEND OTP (optional but useful) -----------------
        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Flash"] = "User not found.";
                return RedirectToAction("Signup");
            }

            if (user.IsVerified)
            {
                TempData["Flash"] = "Account already verified. Please login.";
                return RedirectToAction("Login");
            }

            var rnd = new Random();
            user.Otp = rnd.Next(100000, 999999).ToString();
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);
            await _context.SaveChangesAsync();

            _mail.SendOtp(user.Email, user.Otp!);

            TempData["Flash"] = "A new OTP has been sent to your email.";
            return RedirectToAction("VerifyOtp", new { email = user.Email });
        }

        // ----------------- LOGIN -----------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user == null)
            {
                ViewBag.Message = "Invalid email or password!";
                return View();
            }

            if (!user.IsVerified)
            {
                ViewBag.Message = "Please verify your account first.";
                ViewBag.Email = email;
                return View();
            }

            TempData["Flash"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }
    }
}
