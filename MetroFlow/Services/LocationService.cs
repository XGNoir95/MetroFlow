using System.Text.Json;
using MetroFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace MetroFlow.Services
{
    public class LocationService : ILocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly List<Station> _stations;
        private readonly ApplicationDbContext _db;

        public LocationService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext db)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _db = db;

            // Initialize Dhaka Metro Rail Stations
            _stations = _db.Stations
                    .AsNoTracking()
                    .OrderByDescending(s => s.Latitude)
                    .Select(s => new Station
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        PopularityIndex = s.PopularityIndex  // Make sure this is included
                    })
                    .ToList();
        }

        public List<Station> GetAllStations()
        {
            return _stations.ToList();
        }

        public async Task<List<Place>> SearchPlacesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<Place>();

            try
            {
                var apiKey = _configuration["OpenCage:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                    return new List<Place>();

                var encodedQuery = Uri.EscapeDataString($"{query}, Bangladesh");
                var url = $"https://api.opencagedata.com/geocode/v1/json?q={encodedQuery}&key={apiKey}&limit=8&countrycode=bd&no_annotations=1";

                var response = await _httpClient.GetStringAsync(url);
                var result = JsonSerializer.Deserialize<OpenCageResponse>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (result?.Results == null || !result.Results.Any())
                    return new List<Place>();

                return result.Results.Select(r => new Place
                {
                    Name = r.Formatted ?? string.Empty,
                    DisplayName = FormatDisplayName(r),
                    Latitude = r.Geometry?.Lat ?? 0,
                    Longitude = r.Geometry?.Lng ?? 0
                }).ToList();
            }
            catch (Exception)
            {
                return new List<Place>();
            }
        }

        public Station FindNearestStation(double latitude, double longitude)
        {
            Station nearestStation = null;
            double minDistance = double.MaxValue;

            foreach (var station in _stations)
            {
                double distance = CalculateDistance(latitude, longitude, station.Latitude, station.Longitude);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestStation = new Station
                    {
                        Name = station.Name,
                        Latitude = station.Latitude,
                        Longitude = station.Longitude,
                        Distance = distance
                    };
                }
            }

            return nearestStation ?? throw new InvalidOperationException("No stations available");
        }

        public List<Station> GetRouteStations(Station originStation, Station destinationStation)
        {
            int originIndex = _stations.FindIndex(s => s.Name == originStation.Name);
            int destIndex = _stations.FindIndex(s => s.Name == destinationStation.Name);

            if (originIndex == -1 || destIndex == -1)
            {
                return new List<Station>();
            }

            int start = Math.Min(originIndex, destIndex);
            int end = Math.Max(originIndex, destIndex);

            var routeStations = _stations.GetRange(start, end - start + 1);

            // Reverse if going from south to north (higher index to lower index)
            if (originIndex > destIndex)
            {
                routeStations.Reverse();
            }

            return routeStations;
        }

        public RouteInfo CalculateRoute(Station originStation, Station destinationStation,
            double walkingDistanceToOrigin, double walkingDistanceFromDest)
        {
            var originIndex = _stations.FindIndex(s => s.Name == originStation.Name);
            var destIndex = _stations.FindIndex(s => s.Name == destinationStation.Name);

            var stationCount = Math.Abs(destIndex - originIndex);
            var estimatedTime = stationCount * 2 + 5; // 2 minutes per station + 5 minutes buffer

            var totalWalkingTime = (int)Math.Round((walkingDistanceToOrigin + walkingDistanceFromDest) * 12); // ~12 minutes per km

            var routeStations = GetRouteStations(originStation, destinationStation);

            return new RouteInfo
            {
                Stations = stationCount,
                MetroTime = estimatedTime,
                WalkingTime = totalWalkingTime,
                TotalTime = estimatedTime + totalWalkingTime,
                OriginStation = originStation,
                DestinationStation = destinationStation,
                RouteStations = routeStations
            };
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radius of the Earth in kilometers
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static string FormatDisplayName(OpenCageResult result)
        {
            var components = result.Components;
            if (components == null) return result.Formatted?.Split(',')[0] ?? string.Empty;

            // Primary location name
            var displayName = components.Neighbourhood ?? components.Suburb ?? components.CityDistrict ??
                             components.City ?? components.Town ?? components.Village ??
                             result.Formatted?.Split(',')[0] ?? string.Empty;

            // Add additional context
            var context = string.Empty;
            if (!string.IsNullOrEmpty(components.City) && displayName != components.City)
                context = components.City;
            else if (!string.IsNullOrEmpty(components.StateDistrict) && displayName != components.StateDistrict)
                context = components.StateDistrict;
            else if (!string.IsNullOrEmpty(components.State) && displayName != components.State)
                context = components.State;

            return !string.IsNullOrEmpty(context) ? $"{displayName}, {context}" : displayName;
        }
    }

    // OpenCage API Response Models
    public class OpenCageResponse
    {
        public List<OpenCageResult>? Results { get; set; }
    }

    public class OpenCageResult
    {
        public string? Formatted { get; set; }
        public OpenCageGeometry? Geometry { get; set; }
        public OpenCageComponents? Components { get; set; }
    }

    public class OpenCageGeometry
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class OpenCageComponents
    {
        public string? Neighbourhood { get; set; }
        public string? Suburb { get; set; }
        public string? CityDistrict { get; set; }
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? Village { get; set; }
        public string? StateDistrict { get; set; }
        public string? State { get; set; }
    }
}