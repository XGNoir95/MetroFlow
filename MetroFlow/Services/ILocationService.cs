using MetroFlow.Models;

namespace MetroFlow.Services
{
    public interface ILocationService
    {
        Task<List<Place>> SearchPlacesAsync(string query);
        Station FindNearestStation(double latitude, double longitude);
        RouteInfo CalculateRoute(Station originStation, Station destinationStation,
            double walkingDistanceToOrigin, double walkingDistanceFromDest);
        List<Station> GetAllStations();
        List<Station> GetRouteStations(Station originStation, Station destinationStation);
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    }
}