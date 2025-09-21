using Microsoft.AspNetCore.Mvc;
using MetroFlow.Models;
using MetroFlow.Services;
using System.Collections.Generic;

namespace MetroFlow.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly ITimeZoneService _timeZoneService;

        public ScheduleController(IScheduleService scheduleService, ITimeZoneService timeZoneService)
        {
            _scheduleService = scheduleService;
            _timeZoneService = timeZoneService;
        }

        public IActionResult Index()
        {
            var schedules = _scheduleService.GetSchedules();
            var ticketOptions = _scheduleService.GetTicketOptions();
            var currentTimeInfo = _scheduleService.GetCurrentTimeInfo();

            var viewModel = new ScheduleViewModel
            {
                Schedules = schedules,
                TicketOptions = ticketOptions,
                CurrentTimeInfo = currentTimeInfo
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GetSchedulePeriods(int scheduleId)
        {
            var periods = _scheduleService.GetSchedulePeriods(scheduleId);
            return Json(periods);
        }

        [HttpGet]
        public IActionResult GetCurrentSchedules()
        {
            var currentSchedules = _scheduleService.GetCurrentSchedules();
            return Json(currentSchedules);
        }

        [HttpGet]
        public IActionResult GetCurrentTimeInfo()
        {
            var timeInfo = _scheduleService.GetCurrentTimeInfo();
            return Json(timeInfo);
        }

        [HttpGet]
        public IActionResult GetCurrentPeriodType()
        {
            var periodType = _scheduleService.GetCurrentPeriodType();
            return Json(new { CurrentPeriodType = periodType });
        }
    }

    public class ScheduleViewModel
    {
        public List<Schedule> Schedules { get; set; }
        public List<TicketOption> TicketOptions { get; set; }
        public object CurrentTimeInfo { get; set; }
    }
}
