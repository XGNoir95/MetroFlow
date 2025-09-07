namespace MetroFlow.Models
{
    public class ScheduleModel
    {
        public string Direction { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Frequency { get; set; } // optional for weekdays
    }
}
