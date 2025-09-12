using System;

namespace MetroFlow.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private readonly TimeZoneInfo _bangladeshTimeZone;

        public TimeZoneService()
        {
            try
            {
                // Try to get the system timezone first
                _bangladeshTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time");
            }
            catch
            {
                // Fallback to custom timezone if system timezone not found
                _bangladeshTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                    "Bangladesh Standard Time",
                    TimeSpan.FromHours(6),
                    "Bangladesh Standard Time",
                    "BST"
                );
            }
        }

        public DateTime GetBangladeshTime()
        {
            // Get current UTC time and convert to Bangladesh time
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _bangladeshTimeZone);
        }

        public DateTime ConvertToBangladeshTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, _bangladeshTimeZone);
        }

        public string GetCurrentDayType()
        {
            var bangladeshTime = GetBangladeshTime();
            return bangladeshTime.DayOfWeek == DayOfWeek.Friday ? "Friday" : "Weekday";
        }

        public TimeSpan GetCurrentTime()
        {
            return GetBangladeshTime().TimeOfDay;
        }
    }
}
