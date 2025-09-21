using System;
using System.Collections.Generic;
using System.Linq;
using MetroFlow.Models;

namespace MetroFlow.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ITimeZoneService _timeZoneService;

        public ScheduleService(ITimeZoneService timeZoneService)
        {
            _timeZoneService = timeZoneService;
        }

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
                    FirstTrain = new TimeSpan(7, 0, 0), // Start from 7:00 AM
                    LastTrain = new TimeSpan(21, 40, 0), // End at 9:40 PM
                    PeakFrequency = "Every 8 minutes",
                    IsActive = true
                },
                new Schedule
                {
                    Id = 2,
                    DayType = "Weekday",
                    Direction = "MotijheelToUttara",
                    FirstTrain = new TimeSpan(7, 0, 0), // Start from 7:00 AM
                    LastTrain = new TimeSpan(21, 40, 0), // End at 9:40 PM
                    PeakFrequency = "Every 8 minutes",
                    IsActive = true
                },
                new Schedule
                {
                    Id = 3,
                    DayType = "Friday",
                    Direction = "UttaraToMotijheel",
                    FirstTrain = new TimeSpan(15, 0, 0), // 3:00 PM
                    LastTrain = new TimeSpan(22, 0, 0), // 10:00 PM
                    PeakFrequency = "Every 12 minutes",
                    IsActive = true
                },
                new Schedule
                {
                    Id = 4,
                    DayType = "Friday",
                    Direction = "MotijheelToUttara",
                    FirstTrain = new TimeSpan(15, 0, 0), // 3:00 PM
                    LastTrain = new TimeSpan(22, 0, 0), // 10:00 PM
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
                    1, new List<SchedulePeriod> // Weekday - UttaraToMotijheel
                    {
                        new SchedulePeriod
                        {
                            Id = 1,
                            ScheduleId = 1,
                            StartTime = new TimeSpan(7, 0, 0),   // 7:00 AM
                            EndTime = new TimeSpan(8, 30, 0),   // 8:30 AM
                            PeriodType = "Normal Hour",
                            Frequency = "Every 10 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 2,
                            ScheduleId = 1,
                            StartTime = new TimeSpan(8, 31, 0),  // 8:31 AM
                            EndTime = new TimeSpan(13, 0, 0),   // 1:00 PM
                            PeriodType = "Peak Hour",
                            Frequency = "Every 6 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 3,
                            ScheduleId = 1,
                            StartTime = new TimeSpan(13, 1, 0),  // 1:01 PM
                            EndTime = new TimeSpan(15, 30, 0),  // 3:30 PM
                            PeriodType = "Normal Hour",
                            Frequency = "Every 10 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 4,
                            ScheduleId = 1,
                            StartTime = new TimeSpan(15, 31, 0), // 3:31 PM
                            EndTime = new TimeSpan(20, 30, 0),  // 8:30 PM
                            PeriodType = "Peak Hour",
                            Frequency = "Every 6 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 5,
                            ScheduleId = 1,
                            StartTime = new TimeSpan(20, 31, 0), // 8:31 PM
                            EndTime = new TimeSpan(21, 40, 0),  // 9:40 PM
                            PeriodType = "OffPeak Hour",
                            Frequency = "Every 12 minutes"
                        }
                    }
                },
                {
                    2, new List<SchedulePeriod> // Weekday - MotijheelToUttara
                    {
                        new SchedulePeriod
                        {
                            Id = 6,
                            ScheduleId = 2,
                            StartTime = new TimeSpan(7, 0, 0),   // 7:00 AM
                            EndTime = new TimeSpan(8, 30, 0),   // 8:30 AM
                            PeriodType = "Normal Hour",
                            Frequency = "Every 10 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 7,
                            ScheduleId = 2,
                            StartTime = new TimeSpan(8, 31, 0),  // 8:31 AM
                            EndTime = new TimeSpan(13, 0, 0),   // 1:00 PM
                            PeriodType = "Peak Hour",
                            Frequency = "Every 6 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 8,
                            ScheduleId = 2,
                            StartTime = new TimeSpan(13, 1, 0),  // 1:01 PM
                            EndTime = new TimeSpan(15, 30, 0),  // 3:30 PM
                            PeriodType = "Normal Hour",
                            Frequency = "Every 10 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 9,
                            ScheduleId = 2,
                            StartTime = new TimeSpan(15, 31, 0), // 3:31 PM
                            EndTime = new TimeSpan(20, 30, 0),  // 8:30 PM
                            PeriodType = "Peak Hour",
                            Frequency = "Every 6 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 10,
                            ScheduleId = 2,
                            StartTime = new TimeSpan(20, 31, 0), // 8:31 PM
                            EndTime = new TimeSpan(21, 40, 0),  // 9:40 PM
                            PeriodType = "OffPeak Hour",
                            Frequency = "Every 12 minutes"
                        }
                    }
                },
                {
                    3, new List<SchedulePeriod> // Friday - UttaraToMotijheel
                    {
                        new SchedulePeriod
                        {
                            Id = 11,
                            ScheduleId = 3,
                            StartTime = new TimeSpan(15, 0, 0),  // 3:00 PM
                            EndTime = new TimeSpan(21, 0, 0),   // 9:00 PM
                            PeriodType = "Peak Hour",
                            Frequency = "Every 8 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 12,
                            ScheduleId = 3,
                            StartTime = new TimeSpan(21, 1, 0),  // 9:01 PM
                            EndTime = new TimeSpan(22, 0, 0),   // 10:00 PM
                            PeriodType = "OffPeak Hour",
                            Frequency = "Every 15 minutes"
                        }
                    }
                },
                {
                    4, new List<SchedulePeriod> // Friday - MotijheelToUttara
                    {
                        new SchedulePeriod
                        {
                            Id = 13,
                            ScheduleId = 4,
                            StartTime = new TimeSpan(15, 0, 0),  // 3:00 PM
                            EndTime = new TimeSpan(21, 0, 0),   // 9:00 PM
                            PeriodType = "Peak Hour",
                            Frequency = "Every 8 minutes"
                        },
                        new SchedulePeriod
                        {
                            Id = 14,
                            ScheduleId = 4,
                            StartTime = new TimeSpan(21, 1, 0),  // 9:01 PM
                            EndTime = new TimeSpan(22, 0, 0),   // 10:00 PM
                            PeriodType = "OffPeak Hour",
                            Frequency = "Every 15 minutes"
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

        // Helper method to get current period type based on Bangladesh time and day
        public string GetCurrentPeriodType()
        {
            try
            {
                var currentBangladeshTime = _timeZoneService.GetBangladeshTime();
                var dayType = _timeZoneService.GetCurrentDayType();
                var time = currentBangladeshTime.TimeOfDay;

                if (dayType == "Friday")
                {
                    if (time >= new TimeSpan(21, 1, 0)) // After 9:01 PM
                        return "OffPeak Hour";
                    else if (time >= new TimeSpan(15, 0, 0)) // From 3:00 PM
                        return "Peak Hour";
                    else
                        return "Service Not Available";
                }
                else // Weekday
                {
                    if (time >= new TimeSpan(7, 0, 0) && time <= new TimeSpan(8, 30, 0))
                        return "Normal Hour";
                    else if (time >= new TimeSpan(8, 31, 0) && time <= new TimeSpan(13, 0, 0))
                        return "Peak Hour";
                    else if (time >= new TimeSpan(13, 1, 0) && time <= new TimeSpan(15, 30, 0))
                        return "Normal Hour";
                    else if (time >= new TimeSpan(15, 31, 0) && time <= new TimeSpan(20, 30, 0))
                        return "Peak Hour";
                    else if (time >= new TimeSpan(20, 31, 0) && time <= new TimeSpan(21, 40, 0))
                        return "OffPeak Hour";
                    else
                        return "Service Not Available";
                }
            }
            catch
            {
                return "Service Not Available";
            }
        }

        // Get schedules filtered by current Bangladesh day type
        public List<Schedule> GetCurrentSchedules()
        {
            try
            {
                var currentDayType = _timeZoneService.GetCurrentDayType();
                return GetSchedules().Where(s => s.DayType == currentDayType && s.IsActive).ToList();
            }
            catch
            {
                return new List<Schedule>();
            }
        }

        // Get current Bangladesh time information
        public object GetCurrentTimeInfo()
        {
            try
            {
                var bangladeshTime = _timeZoneService.GetBangladeshTime();
                var dayType = _timeZoneService.GetCurrentDayType();
                var currentPeriodType = GetCurrentPeriodType();

                return new
                {
                    CurrentTime = bangladeshTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeZone = "Bangladesh Standard Time (UTC+6)",
                    DayType = dayType,
                    CurrentPeriodType = currentPeriodType,
                    TimeOfDay = $"{bangladeshTime.Hour:D2}:{bangladeshTime.Minute:D2}:{bangladeshTime.Second:D2}",
                    FormattedTime = bangladeshTime.ToString("hh:mm:ss tt"),
                    Hour = bangladeshTime.Hour,
                    Minute = bangladeshTime.Minute,
                    Second = bangladeshTime.Second
                };
            }
            catch (Exception ex)
            {
                // Return safe default values in case of error
                return new
                {
                    CurrentTime = DateTime.UtcNow.AddHours(6).ToString("yyyy-MM-dd HH:mm:ss"),
                    TimeZone = "Bangladesh Standard Time (UTC+6)",
                    DayType = "Weekday",
                    CurrentPeriodType = "Service Not Available",
                    TimeOfDay = "00:00:00",
                    FormattedTime = "12:00:00 AM",
                    Hour = 0,
                    Minute = 0,
                    Second = 0,
                    Error = ex.Message
                };
            }
        }
    }
}
