using MetroFlow.Models;
using MetroFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetroFlow.Controllers
{
    // The base route for all admin-related actions.
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        private readonly LostAndFoundService _lostAndFoundService;
        private readonly DistressService _DistressService;
        private readonly TokenService _tokens;

        public AdminController(
            AdminService adminService,
            LostAndFoundService lostAndFoundService,
            DistressService DistressService,
            TokenService tokens)
        {
            _adminService = adminService;
            _lostAndFoundService = lostAndFoundService;
            _DistressService = DistressService;
            _tokens = tokens;
        }

        // ======================= Admin Authentication =======================

        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var admin = await _adminService.ValidateAdminCredentialsAsync(username, password);

            if (admin == null)
            {
                ViewBag.Message = "Invalid username or password!";
                return View();
            }

            // Create JWT and store in HttpOnly cookie
            var jwt = _tokens.CreateAdminToken(admin); // Assuming a separate method for admin tokens
            Response.Cookies.Append("AdminAuthToken", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // true in production (HTTPS)
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(12),
                IsEssential = true
            });

            TempData["Flash"] = "Admin login successful!";
            return RedirectToAction("Dashboard");
        }

        [HttpPost("logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AdminAuthToken");
            TempData["Flash"] = "Logged out.";
            return RedirectToAction("Login");
        }

        // ======================= Admin Dashboard =======================

        // This is the main admin dashboard view.
        // The [Authorize] attribute ensures only authenticated users can access this.
        [HttpGet("dashboard")]
        [Authorize]
        public IActionResult Dashboard()
        {
            // You can load some summary data for the dashboard here
            return View();
        }

        // ======================= Admin Management =======================

        // Displays a list of all admins.
        [HttpGet("admins")]
        [Authorize]
        public async Task<IActionResult> Admins()
        {
            // This is a placeholder as AdminService doesn't have a GetAllAdmins method.
            // You would need to add one. For now, we use a direct context call.
            var admins = await _adminService.GetAllAdminsAsync();
            return View(admins);
        }

        // Displays the form to create a new admin.
        [HttpGet("create")]
        [Authorize]
        public IActionResult Create() => View();

        // Handles the form submission for creating a new admin.
        [HttpPost("create")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Admin admin, string password)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid data. Please check your inputs.";
                return View(admin);
            }

            try
            {
                await _adminService.CreateAdminAsync(admin, password);
                TempData["Flash"] = "Admin created successfully!";
                return RedirectToAction("Admins");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error creating admin: {ex.Message}";
                return View(admin);
            }
        }

        // Displays the form to edit an existing admin.
        [HttpGet("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var admin = await _adminService.GetAdminByIdAsync(id); // Assumes a GetAdminByIdAsync method
                return View(admin);
            }
            catch (KeyNotFoundException)
            {
                TempData["Flash"] = "Admin not found.";
                return RedirectToAction("Admins");
            }
        }

        // Handles the form submission for editing an existing admin.
        [HttpPost("edit/{id}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Admin updatedAdmin, string? newPassword)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid data. Please check your inputs.";
                return View(updatedAdmin);
            }

            try
            {
                await _adminService.UpdateAdminAsync(id, updatedAdmin, newPassword);
                TempData["Flash"] = "Admin updated successfully!";
                return RedirectToAction("Admins");
            }
            catch (KeyNotFoundException)
            {
                TempData["Flash"] = "Admin not found. Update failed.";
                return RedirectToAction("Admins");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error updating admin: {ex.Message}";
                return View(updatedAdmin);
            }
        }

        // Handles the deletion of an admin.
        [HttpPost("delete/{id}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _adminService.DeleteAdminAsync(id);
                TempData["Flash"] = "Admin deleted successfully!";
            }
            catch (KeyNotFoundException)
            {
                TempData["Flash"] = "Admin not found.";
            }
            catch (Exception ex)
            {
                TempData["Flash"] = $"Error deleting admin: {ex.Message}";
            }
            return RedirectToAction("Admins");
        }

        // ======================= Lost and Found Management =======================

        [HttpGet("lostitems")]
        [Authorize]
        public async Task<IActionResult> LostItems()
        {
            var lostItems = await _lostAndFoundService.GetAllLostItemsAsync();
            return View(lostItems);
        }

        [HttpPost("lostitems/update-status/{id}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLostItemStatus(int id, bool isClaimed)
        {
            try
            {
                await _lostAndFoundService.UpdateLostItemStatusAsync(id, isClaimed);
                TempData["Flash"] = $"Lost item status updated to '{(isClaimed ? "Claimed" : "Unclaimed")}' successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["Flash"] = "Lost item not found.";
            }
            catch (Exception ex)
            {
                TempData["Flash"] = $"Error updating lost item status: {ex.Message}";
            }
            return RedirectToAction("LostItems");
        }

        // ======================= Distress Signal Management =======================

        [HttpGet("distress")]
        [Authorize]
        public async Task<IActionResult> DistressSignals()
        {
            var distressSignals = await _DistressService.GetAllDistressesAsync();
            return View(distressSignals);
        }

        [HttpPost("distress/delete/{id}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDistressSignal(int id)
        {
            try
            {
                await _DistressService.DeleteDistressAsync(id);
                TempData["Flash"] = "Distress signal resolved and deleted.";
            }
            catch (KeyNotFoundException)
            {
                TempData["Flash"] = "Distress signal not found.";
            }
            catch (Exception ex)
            {
                TempData["Flash"] = $"Error deleting distress signal: {ex.Message}";
            }
            return RedirectToAction("DistressSignals");
        }
    }
}