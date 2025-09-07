namespace MetroFlow.Models
{
    public class RouteInfo
    {
        public int Stations { get; set; }
        public int MetroTime { get; set; }
        public int WalkingTime { get; set; }
        public int TotalTime { get; set; }
        public Station? OriginStation { get; set; }
        public Station? DestinationStation { get; set; }
        public List<Station> RouteStations { get; set; } = new();
    }
}