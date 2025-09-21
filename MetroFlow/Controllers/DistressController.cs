using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetroFlow.Models;
using MetroFlow.Services;

namespace MetroFlow.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DistressController : ControllerBase
    {
        private readonly DistressService _distressService;

        public DistressController(DistressService distressService)
        {
            _distressService = distressService;
        }

        // POST: api/distress/report
        [HttpPost("report")]
        public async Task<IActionResult> Report([FromBody] Distress distress)
        {
            if (string.IsNullOrEmpty(distress.EmergencyType) || string.IsNullOrEmpty(distress.StationName))
            {
                return BadRequest("Emergency type and station name are required.");
            }

            distress.UserId = int.Parse(User.Identity?.Name ?? "0");
            distress.ReportedAt = DateTime.UtcNow;
            distress.Status = "Pending";

            await _distressService.AddDistressAsync(distress);

            return Ok(new { message = "Distress report submitted successfully", reportId = distress.DistressId });
        }

        // GET: api/distress/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var distress = await _distressService.GetDistressAsync(id);
            if (distress == null) return NotFound();

            return Ok(distress);
        }

        // GET: api/distress
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var distresses = await _distressService.GetAllDistressesAsync();
            return Ok(distresses);
        }

        // PUT: api/distress/update
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] Distress updated)
        {
            var success = await _distressService.UpdateDistressAsync(updated);
            if (!success) return NotFound();

            return Ok(new { message = "Distress updated successfully" });
        }

        // DELETE: api/distress/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _distressService.DeleteDistressAsync(id);
            if (!success) return NotFound();

            return Ok(new { message = "Distress deleted successfully" });
        }
    }
}
