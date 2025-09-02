using MetroFlow.Models;
using MetroFlow.Services;
using Microsoft.AspNetCore.Identity;
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

        // =================== Signup (GET) ===================
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        // =================== Signup (POST) ===================
        [HttpPost]
        public async Task<IActionResult> Signup(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid data. Please check your inputs.";
                ViewBag.Users = await _context.Users.ToListAsync();
                return View(user);
            }

            // check duplicate email
            var exists = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (exists)
            {
                ViewBag.Message = "Email already registered!";
                ViewBag.Users = await _context.Users.ToListAsync();
                return View(user);
            }

            user.CreatedAt = DateTime.UtcNow;

            // ✅ Hash password
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            // Generate OTP
            var rnd = new Random();
            user.Otp = rnd.Next(100000, 999999).ToString();
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(3);
            user.IsVerified = false;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send OTP via email
            _mail.SendOtp(user.Email, user.Otp!);

            TempData["Flash"] = "We emailed you an OTP. Please verify.";
            return RedirectToAction("VerifyOtp", new { email = user.Email });
        }

        // =================== Verify OTP (GET) ===================
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // =================== Verify OTP (POST) ===================
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "User not found!";
                return View();
            }

            if (user.Otp != otp || user.OtpExpiry < DateTime.UtcNow)
            {
                ViewBag.Message = "Invalid or expired OTP!";
                ViewBag.Email = email;
                return View();
            }

            user.IsVerified = true;
            user.Otp = null;
            user.OtpExpiry = null;

            await _context.SaveChangesAsync();

            TempData["Flash"] = "Your account has been verified. Please log in.";
            return RedirectToAction("Login");
        }

        // =================== Resend OTP ===================
        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "User not found!";
                return RedirectToAction("Signup");
            }

            var rnd = new Random();
            user.Otp = rnd.Next(100000, 999999).ToString();
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(3);

            await _context.SaveChangesAsync();

            _mail.SendOtp(user.Email, user.Otp!);

            TempData["Flash"] = "A new OTP has been sent.";
            return RedirectToAction("VerifyOtp", new { email = user.Email });
        }

        // =================== Login (GET) ===================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =================== Login (POST) ===================
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Invalid email or password!";
                return View();
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Failed)
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
