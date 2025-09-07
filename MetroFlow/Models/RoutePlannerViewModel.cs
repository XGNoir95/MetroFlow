// Models/RoutePlannerViewModel.cs
namespace MetroFlow.Models
{
    public class RoutePlannerViewModel
    {
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public string CurrentLocationText { get; set; } = "Get Current Location";
        public Station? NearestCurrentStation { get; set; }

        // Origin properties
        public string OriginQuery { get; set; } = string.Empty;
        public List<Place> OriginSuggestions { get; set; } = new();
        public Place? SelectedOrigin { get; set; }
        public Station? NearestOriginStation { get; set; }

        // Destination properties
        public string DestinationQuery { get; set; } = string.Empty;
        public List<Place> DestinationSuggestions { get; set; } = new();
        public Place? SelectedDestination { get; set; }
        public Station? NearestDestinationStation { get; set; }

        public RouteInfo? RouteInfo { get; set; }
        public List<Station> AllStations { get; set; } = new();

        public bool HasCurrentLocation => CurrentLatitude.HasValue && CurrentLongitude.HasValue;
        public bool HasOrigin => SelectedOrigin != null;
        public bool HasDestination => SelectedDestination != null;
        public bool CanPlanRoute => (HasCurrentLocation || HasOrigin) && HasDestination &&
                                   (NearestCurrentStation != null || NearestOriginStation != null) &&
                                   NearestDestinationStation != null;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}