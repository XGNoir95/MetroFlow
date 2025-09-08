using Azure;
using MetroFlow.Controllers;
using MetroFlow.Models;
using MetroFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MetroFlow.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MailService _mail;
        private readonly TokenService _tokens;

        public UserController(ApplicationDbContext context, MailService mail, TokenService tokens)
        {
            _context = context;
            _mail = mail;
            _tokens = tokens;
        }

        // =================== Signup (GET) ===================
        [HttpGet, AllowAnonymous]
        public IActionResult Signup() => View();

        // =================== Signup (POST) ===================
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid data. Please check your inputs.";
                return View(user);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                if (existingUser.IsVerified)
                {
                    ViewBag.Message = "Email already registered!";
                    return View(user);
                }
                else
                {
                    ViewBag.Message = "Your account is not verified.";
                    ViewBag.ShowVerifyButton = true;
                    ViewBag.Email = existingUser.Email;
                    return View(user);
                }
            }

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            user.CreatedAt = DateTime.UtcNow;
            user.IsVerified = false;
            var rnd = new Random();
            user.Otp = rnd.Next(100000, 999999).ToString();
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mail.SendOtp(user.Email, user.Otp!);

            TempData["Flash"] = "We emailed you an OTP. Please verify.";
            return RedirectToAction("VerifyOtp", new { email = user.Email });
        }

        // =================== Verify OTP (GET) ===================
        [HttpGet, AllowAnonymous]
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // =================== Verify OTP (POST) ===================
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
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
            _mail.SendConfirmation(user.Email, user.Name);

            TempData["Flash"] = "Your account has been verified. Please log in.";
            return RedirectToAction("Login");
        }

        // =================== Resend OTP ===================
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Flash"] = "User not found!";
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

        // =================== Login (GET) ===================
        [HttpGet, AllowAnonymous]
        public IActionResult Login() => View();

        // =================== Login (POST) -> set HttpOnly cookie and REDIRECT ===================
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Invalid email or password!";
                return View();
            }

            var hasher = new PasswordHasher<User>();
            if (hasher.VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Failed)
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

            // Create JWT and store in HttpOnly cookie
            var jwt = _tokens.CreateToken(user);
            Response.Cookies.Append("AuthToken", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,                 // true in production (HTTPS)
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(12),
                IsEssential = true
            });

            TempData["Flash"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }

        // =================== Logout -> delete cookie and REDIRECT ===================
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            TempData["Flash"] = "Logged out.";
            return RedirectToAction("Login");
        }
    }
}
