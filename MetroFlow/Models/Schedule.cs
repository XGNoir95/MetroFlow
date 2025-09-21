using System;

namespace MetroFlow.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public string DayType { get; set; } // "Weekday", "Friday"
        public string Direction { get; set; } // "UttaraToMotijheel", "MotijheelToUttara"
        public TimeSpan FirstTrain { get; set; }
        public TimeSpan LastTrain { get; set; }
        public string PeakFrequency { get; set; }
        public bool IsActive { get; set; }
    }
}
