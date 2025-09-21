using Microsoft.AspNetCore.Mvc;
using MetroFlow.Models;
using MetroFlow.Services;
using System.Linq;

namespace MetroFlow.Controllers
{
    public class TouristSpotsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TouristSpotsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult TouristAttractions()
        {
            var stations = _db.Stations.Select(s => new Station
            {
                Name = s.Name,
                Latitude = s.Latitude,
                Longitude = s.Longitude
            }).ToList();

            return View(stations);
        }
    }
}