using System.Text.Json;
using MetroFlow.Models;

namespace MetroFlow.Services
{
    public class LocationService : ILocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly List<Station> _stations;

        public LocationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            // Initialize Dhaka Metro Rail Stations
            _stations = new List<Station>
            {
                new() { Name = "Uttara North", Latitude = 23.8759, Longitude = 90.3795 },
                new() { Name = "Uttara Center", Latitude = 23.8697, Longitude = 90.3831 },
                new() { Name = "Uttara South", Latitude = 23.8643, Longitude = 90.3867 },
                new() { Name = "Pallabi", Latitude = 23.8279, Longitude = 90.3651 },
                new() { Name = "Mirpur 11", Latitude = 23.8223, Longitude = 90.3651 },
                new() { Name = "Mirpur 10", Latitude = 23.8067, Longitude = 90.3651 },
                new() { Name = "Kazipara", Latitude = 23.7967, Longitude = 90.3651 },
                new() { Name = "Shewrapara", Latitude = 23.7867, Longitude = 90.3651 },
                new() { Name = "Agargaon", Latitude = 23.7767, Longitude = 90.3751 },
                new() { Name = "Bijoy Sarani", Latitude = 23.7667, Longitude = 90.3851 },
                new() { Name = "Farmgate", Latitude = 23.7567, Longitude = 90.3951 },
                new() { Name = "Karwan Bazar", Latitude = 23.7467, Longitude = 90.4051 },
                new() { Name = "Shahbagh", Latitude = 23.7367, Longitude = 90.4151 },
                new() { Name = "Dhaka University", Latitude = 23.7267, Longitude = 90.4251 },
                new() { Name = "Secretariat", Latitude = 23.7167, Longitude = 90.4351 },
                new() { Name = "Motijheel", Latitude = 23.7067, Longitude = 90.4451 }
            };
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
            double minDistance = double.MaxValue;
            Station? nearest = null;

            foreach (var station in _stations)
            {
                var distance = CalculateDistance(latitude, longitude, station.Latitude, station.Longitude);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = new Station
                    {
                        Name = station.Name,
                        Latitude = station.Latitude,
                        Longitude = station.Longitude,
                        Distance = distance
                    };
                }
            }

            return nearest ?? throw new InvalidOperationException("No stations available");
        }

        public List<Station> GetRouteStations(Station originStation, Station destinationStation)
        {
            if (originStation == null || destinationStation == null)
                return new List<Station>();

            var originIndex = _stations.FindIndex(s => s.Name == originStation.Name);
            var destIndex = _stations.FindIndex(s => s.Name == destinationStation.Name);

            if (originIndex == -1 || destIndex == -1)
                return new List<Station>();

            var start = Math.Min(originIndex, destIndex);
            var end = Math.Max(originIndex, destIndex);

            return _stations.GetRange(start, end - start + 1);
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