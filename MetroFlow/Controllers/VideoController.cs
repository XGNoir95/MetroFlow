using MetroFlow.Models;
using MetroFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetroFlow.Controllers
{
    public class VideoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VideoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MetYube()
        {
            var videos = await _context.Videos.ToListAsync();
            return View(videos);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }
            return View(video);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}