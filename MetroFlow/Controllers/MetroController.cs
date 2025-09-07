using MetroFlow.Models;
using Microsoft.AspNetCore.Mvc;

namespace MetroFlow.Controllers
{
    public class MetroController : Controller
    {
        public IActionResult Schedule()
        {
            // Example schedules - replace with real DB data
            var weekdaySchedules = new List<ScheduleModel>
            {
                new ScheduleModel { Direction = "Northbound", StartTime = "7:30 AM", EndTime = "9:40 PM" },
                new ScheduleModel { Direction = "Southbound", StartTime = "7:45 AM", EndTime = "9:55 PM" }
            };

            var fridaySchedules = new List<ScheduleModel>
            {
                new ScheduleModel { Direction = "Northbound", StartTime = "3:00 PM", EndTime = "10:20 PM", Frequency = "Every 20 mins" },
                new ScheduleModel { Direction = "Southbound", StartTime = "3:10 PM", EndTime = "10:30 PM", Frequency = "Every 25 mins" }
            };

            ViewBag.WeekdaySchedules = weekdaySchedules;
            ViewBag.FridaySchedules = fridaySchedules;

            return View();
        }
    }
}
