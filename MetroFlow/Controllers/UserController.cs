using Azure;
using MetroFlow.Controllers;
using MetroFlow.Models;
using MetroFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MetroFlow.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MailService _mail;
        private readonly TokenService _tokens;
        private readonly IMemoryCache _cache;

        public UserController(ApplicationDbContext context, MailService mail, TokenService tokens, IMemoryCache cache)
        {
            _context = context;
            _mail = mail;
            _tokens = tokens;
            _cache = cache;
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

         // =================== Forgot Password (GET) ===================
        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        // =================== Forgot Password (POST) ===================
        // UserController.cs

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Message = "Please provide your email.";
                return View();
            }

            // Check if the email exists in the database
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !user.IsVerified)
            { 
                TempData["Flash"] = "Wrong Email or Email not registered.";
                return RedirectToAction("ForgotPassword");
            }

            // Generate a random token
            var token = Guid.NewGuid().ToString("N"); // Raw token

            // Cache the token with an expiration of 30 minutes
            _cache.Set($"passwordReset:{email}:{token}", email, TimeSpan.FromMinutes(5));

            // Generate the reset link
            var resetLink = Url.Action("ResetPassword", "User", new { email = email, token = token }, Request.Scheme);

            // Send the reset link via email
            _mail.SendPasswordReset(email, resetLink);

            TempData["Flash"] = "Password reset mail is sent.";
            return RedirectToAction("ForgotPassword");
        }


        // =================== Reset Password (GET) ===================
        [HttpGet, AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        // =================== Reset Password (POST) ===================
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string email, string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
                return View();
            }

            // Check if the token exists in the cache
            var cacheKey = $"passwordReset:{email}:{token}";
            if (!_cache.TryGetValue(cacheKey, out string cachedEmail) || cachedEmail != email)
            {
                ViewBag.Message = "Invalid or expired reset link.";
                return View();
            }

            // Find the user and update password
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "User not found.";
                return View();
            }

            // Hash the new password
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, newPassword);

            // Remove the token from the cache (single-use)
            _cache.Remove(cacheKey);

            // Save the changes
            _context.SaveChanges();

            TempData["Flash"] = "Your password has been reset. Please log in.";
            return RedirectToAction("Login");
        }
    }
}
