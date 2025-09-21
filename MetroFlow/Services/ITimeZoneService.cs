using System;

namespace MetroFlow.Services
{
    public interface ITimeZoneService
    {
        DateTime GetBangladeshTime();
        DateTime ConvertToBangladeshTime(DateTime utcTime);
        string GetCurrentDayType();
        TimeSpan GetCurrentTime();
    }
}
