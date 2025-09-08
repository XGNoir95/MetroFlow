using Microsoft.AspNetCore.Mvc;
using MetroFlow.Models;
using MetroFlow.Services;
namespace MetroFlow.Services
{
    public class ScheduleService : IScheduleService
    {
        // Since we dont have accurate data and api, this is hardcoded :))
        public List<Schedule> GetSchedules()
        {
            return new List<Schedule>
            {
                new Schedule
                {
                    Id = 1,
                    DayType = "Weekday",
                    Direction = "UttaraToMotijheel",
                    FirstTrain = new TimeSpan(7, 10, 0),
                    LastTrain = new TimeSpan(21, 0, 0),
                    PeakFrequency = "Every 8 minutes",
                    IsActive = true
                },
                new Schedule
                {
                    Id = 2,
                    DayType = "Weekday",
                    Direction = "MotijheelToUttara",
                    FirstTrain = new TimeSpan(7, 30, 0),
                    LastTrain = new TimeSpan(21, 40, 0),
                    PeakFrequency = "Every 8 minutes",
                    IsActive = true
                },
                new Schedule
                {
                    Id = 3,
                    DayType = "Friday",
                    Direction = "UttaraToMotijheel",
                    FirstTrain = new TimeSpan(15, 0, 0),
                    LastTrain = new TimeSpan(22, 0, 0),
                    PeakFrequency = "Every 12 minutes",
                    IsActive = true
                },
                new Schedule
                {
                    Id = 4,
                    DayType = "Friday",
                    Direction = "MotijheelToUttara",
                    FirstTrain = new TimeSpan(15, 30, 0),
                    LastTrain = new TimeSpan(22, 20, 0),
                    PeakFrequency = "Every 12 minutes",
                    IsActive = true
                }
            };
        }

        public List<SchedulePeriod> GetSchedulePeriods(int scheduleId)
        {
            // This would typically come from a database
            var periods = new Dictionary<int, List<SchedulePeriod>>
            {
                {
                    1, new List<SchedulePeriod>
                    {
                        new SchedulePeriod
                        {
                            Id = 1,
                            ScheduleId = 1,
                            StartTime = new TimeSpan(7, 10, 0),
                            EndTime = new TimeSpan(7, 30, 0),
                            PeriodType = "Off-Peak",
                            Frequency = "Every 10 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 2,
                            ScheduleId = 1,
                            StartTime = new TimeSpan(7, 31, 0),
                            EndTime = new TimeSpan(18, 50, 0),
                            PeriodType = "Peak",
                            Frequency = "Every 8 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 3,
                            ScheduleId = 1,
                            StartTime = new TimeSpan(18, 51, 0),
                            EndTime = new TimeSpan(21, 0, 0),
                            PeriodType = "Off-Peak",
                            Frequency = "Every 10 minutes"
                        }
                    }
                },
                {
                    2, new List<SchedulePeriod>
                    {
                        new SchedulePeriod
                        {
                            Id = 4,
                            ScheduleId = 2,
                            StartTime = new TimeSpan(7, 30, 0),
                            EndTime = new TimeSpan(8, 0, 0),
                            PeriodType = "Off-Peak",
                            Frequency = "Every 10 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 5,
                            ScheduleId = 2,
                            StartTime = new TimeSpan(8, 1, 0),
                            EndTime = new TimeSpan(19, 30, 0),
                            PeriodType = "Peak",
                            Frequency = "Every 8 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 6,
                            ScheduleId = 2,
                            StartTime = new TimeSpan(19, 31, 0),
                            EndTime = new TimeSpan(21, 40, 0),
                            PeriodType = "Off-Peak",
                            Frequency = "Every 10 minutes"
                        }
                    }
                },
                {
                    3, new List<SchedulePeriod>
                    {
                        new SchedulePeriod
                        {
                            Id = 7,
                            ScheduleId = 3,
                            StartTime = new TimeSpan(15, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0),
                            PeriodType = "All Hours",
                            Frequency = "Every 12 minutes"
                        }
                    }
                },
                {
                    4, new List<SchedulePeriod>
                    {
                        new SchedulePeriod
                        {
                            Id = 8,
                            ScheduleId = 4,
                            StartTime = new TimeSpan(15, 20, 0),
                            EndTime = new TimeSpan(22, 20, 0),
                            PeriodType = "All Hours",
                            Frequency = "Every 12 minutes"
                        }
                    }
                }
            };

            return periods.ContainsKey(scheduleId) ? periods[scheduleId] : new List<SchedulePeriod>();
        }

        public List<TicketOption> GetTicketOptions()
        {
            return new List<TicketOption>
            {
                new TicketOption
                {
                    Id = 1,
                    Type = "SingleJourney",
                    Title = "Single Journey Ticket",
                    Description = "Valid for a one-time trip between two stations.",
                    PurchaseMethods = new List<PurchaseMethod>
                    {
                        new PurchaseMethod
                        {
                            Id = 1,
                            Name = "Physical Purchase",
                            Description = "Go to a counter and book a ticket",
                            IconClass = "fas fa-university"
                        },
                        new PurchaseMethod
                        {
                            Id = 2,
                            Name = "Online Portal",
                            Description = "Book tickets at metroflow.com",
                            IconClass = "fas fa-globe"
                        }
                    }
                },
                new TicketOption
                {
                    Id = 2,
                    Type = "MRTPass",
                    Title = "MRT Pass",
                    Description = "A contactless smart card offering a 10% discount on fares. Valid for 10 years.",
                    PurchaseMethods = new List<PurchaseMethod>
                    {
                        new PurchaseMethod
                        {
                            Id = 3,
                            Name = "Physical Purchase",
                            Description = "Go to a counter and register for your pass",
                            IconClass = "fas fa-university"
                        },
                        new PurchaseMethod
                        {
                            Id = 4,
                            Name = "Online Portal",
                            Description = "Register for your card on Metroflow",
                            IconClass = "fas fa-globe"
                        }
                    }
                }
            };
        }
    }
}