// Controllers/ScheduleController.cs
using Microsoft.AspNetCore.Mvc;
using MetroFlow.Models;
using MetroFlow.Services;

namespace MetroFlow.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        public IActionResult Index()
        {
            var schedules = _scheduleService.GetSchedules();
            var ticketOptions = _scheduleService.GetTicketOptions();

            var viewModel = new ScheduleViewModel
            {
                Schedules = schedules,
                TicketOptions = ticketOptions
            };

            return View(viewModel);
        }

        public IActionResult GetSchedulePeriods(int scheduleId)
        {
            var periods = _scheduleService.GetSchedulePeriods(scheduleId);
            return Json(periods);
        }
    }

    public class ScheduleViewModel
    {
        public List<Schedule> Schedules { get; set; }
        public List<TicketOption> TicketOptions { get; set; }
    }
}