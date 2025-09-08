namespace MetroFlow.Models
{
    public class SchedulePeriod
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string PeriodType { get; set; } // "Peak", "Off-Peak"
        public string Frequency { get; set; }
    }
}