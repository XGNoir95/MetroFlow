using Microsoft.AspNetCore.Mvc;
using MetroFlow.Models;
using MetroFlow.Services;
namespace MetroFlow.Services
{
    public interface IScheduleService
    {
        List<Schedule> GetSchedules();
        List<SchedulePeriod> GetSchedulePeriods(int scheduleId);
        List<TicketOption> GetTicketOptions();
    }
}