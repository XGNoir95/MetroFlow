using System;

namespace MetroFlow.Models
{
    public class SchedulePeriod
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string PeriodType { get; set; } // "Peak Hour", "Normal Hour", "OffPeak Hour"
        public string Frequency { get; set; }
    }
}
