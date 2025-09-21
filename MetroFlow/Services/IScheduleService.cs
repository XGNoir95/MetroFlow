using System.Collections.Generic;
using MetroFlow.Models;

namespace MetroFlow.Services
{
    public interface IScheduleService
    {
        List<Schedule> GetSchedules();
        List<SchedulePeriod> GetSchedulePeriods(int scheduleId);
        List<TicketOption> GetTicketOptions();
        string GetCurrentPeriodType();
        List<Schedule> GetCurrentSchedules();
        object GetCurrentTimeInfo();
    }
}
