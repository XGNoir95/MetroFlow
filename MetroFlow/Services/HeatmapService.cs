using MetroFlow.Models;

namespace MetroFlow.Services
{
    public class HeatmapService : IHeatmapService
    {
        private readonly ITimeZoneService _timeZoneService;
        private readonly IScheduleService _scheduleService;
        private readonly ILocationService _locationService;

        public HeatmapService(ITimeZoneService timeZoneService, IScheduleService scheduleService, ILocationService locationService)
        {
            _timeZoneService = timeZoneService;
            _scheduleService = scheduleService;
            _locationService = locationService;
        }

        public object GetStationHeatmapData(Station station, string currentPeriod)
        {
            var heatIntensity = CalculateHeatIntensity(station.PopularityIndex, currentPeriod);
            var color = GetHeatmapColor(station.PopularityIndex, currentPeriod);

            return new
            {
                StationName = station.Name,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                PopularityIndex = station.PopularityIndex,
                CurrentPeriod = currentPeriod,
                HeatIntensity = heatIntensity,
                Color = color,
                Radius = CalculateRadius(station.PopularityIndex, currentPeriod),
                Description = GetStationHeatmapDescription(station, currentPeriod)
            };
        }

        public List<object> GetAllStationsHeatmapData(string currentPeriod)
        {
            var stations = GetStationsWithPopularity();
            return stations.Select(station => GetStationHeatmapData(station, currentPeriod)).ToList();
        }

        public List<object> GetTimePeriodOptions()
        {
            return new List<object>
            {
                new { Value = "current", Label = "Current Time", DayType = "Current" },
                new { Value = "weekday-7:00-8:30", Label = "7:00 AM - 8:30 AM", DayType = "Weekday" },
                new { Value = "weekday-8:31-13:00", Label = "8:31 AM - 1:00 PM", DayType = "Weekday" },
                new { Value = "weekday-13:01-15:30", Label = "1:01 PM - 3:30 PM", DayType = "Weekday" },
                new { Value = "weekday-15:31-20:30", Label = "3:31 PM - 8:30 PM", DayType = "Weekday" },
                new { Value = "weekday-20:31-21:40", Label = "8:31 PM - 9:40 PM", DayType = "Weekday" },
                new { Value = "friday-15:00-21:00", Label = "3:00 PM - 9:00 PM", DayType = "Friday" },
                new { Value = "friday-21:01-22:00", Label = "9:01 PM - 10:00 PM", DayType = "Friday" }
            };
        }

        public string GetPeriodTypeFromTimeRange(string timeRange, string dayType)
        {
            if (timeRange == "current")
            {
                return _scheduleService.GetCurrentPeriodType();
            }

            return timeRange switch
            {
                "weekday-7:00-8:30" => "Normal Hour",
                "weekday-8:31-13:00" => "Peak Hour",
                "weekday-13:01-15:30" => "Normal Hour",
                "weekday-15:31-20:30" => "Peak Hour",
                "weekday-20:31-21:40" => "OffPeak Hour",
                "friday-15:00-21:00" => "Peak Hour",
                "friday-21:01-22:00" => "OffPeak Hour",
                _ => "Service Not Available"
            };
        }

        private string GetStationHeatmapDescription(Station station, string currentPeriod)
        {
            var popularityLevel = GetPopularityLevel(station.PopularityIndex);
            var activityDescription = GetActivityDescription(currentPeriod);

            return currentPeriod switch
            {
                "Peak Hour" => $"High activity zone with {popularityLevel} ridership. Expect crowded platforms and frequent train arrivals. {activityDescription}",
                "Normal Hour" => $"Moderate activity with {popularityLevel} passenger flow. Balanced service frequency and manageable crowd levels. {activityDescription}",
                "OffPeak Hour" => $"Low activity period with {popularityLevel} usage patterns. Longer intervals between trains and minimal crowding. {activityDescription}",
                "Service Not Available" => $"Metro service is currently inactive. Station shows {popularityLevel} during operational hours.",
                _ => $"Activity level varies with {popularityLevel} depending on service period."
            };
        }

        private string GetPopularityLevel(int popularityIndex)
        {
            return popularityIndex switch
            {
                >= 14 => "extremely high",
                >= 11 => "very high",
                >= 8 => "high",
                >= 5 => "moderate",
                >= 3 => "low",
                _ => "minimal"
            };
        }

        private string GetActivityDescription(string period)
        {
            return period switch
            {
                "Peak Hour" => "Prime commuting hours with maximum capacity utilization.",
                "Normal Hour" => "Regular operational period with steady passenger movement.",
                "OffPeak Hour" => "Quiet period ideal for comfortable travel.",
                _ => "Variable activity depending on time and day."
            };
        }

        private double CalculateHeatIntensity(int popularityIndex, string currentPeriod)
        {
            double baseIntensity = currentPeriod switch
            {
                "Peak Hour" => 1.0,
                "Normal Hour" => 0.7,
                "OffPeak Hour" => 0.4,
                _ => 0.2
            };

            double popularityMultiplier = 0.3 + (0.7 * popularityIndex / 16.0);
            return Math.Round(baseIntensity * popularityMultiplier, 2);
        }

        private string GetHeatmapColor(int popularityIndex, string currentPeriod)
        {
            var baseColor = currentPeriod switch
            {
                "Peak Hour" => new { R = 255, G = 0, B = 0 },      // Red
                "Normal Hour" => new { R = 255, G = 165, B = 0 },   // Orange
                "OffPeak Hour" => new { R = 0, G = 255, B = 0 },    // Green
                _ => new { R = 128, G = 128, B = 128 }              // Gray
            };

            double intensity = 0.3 + (0.7 * popularityIndex / 16.0);
            int r = (int)(baseColor.R * intensity);
            int g = (int)(baseColor.G * intensity);
            int b = (int)(baseColor.B * intensity);

            return $"rgb({r},{g},{b})";
        }

        private int CalculateRadius(int popularityIndex, string currentPeriod)
        {
            int baseRadius = currentPeriod switch
            {
                "Peak Hour" => 300,
                "Normal Hour" => 250,
                "OffPeak Hour" => 200,
                _ => 150
            };

            double popularityMultiplier = 0.5 + (0.5 * popularityIndex / 16.0);
            return (int)(baseRadius * popularityMultiplier);
        }

        private List<Station> GetStationsWithPopularity()
        {
            return _locationService.GetAllStations();
        }
    }
}
