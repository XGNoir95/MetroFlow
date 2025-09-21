namespace MetroFlow.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Distance { get; set; }
        public int PopularityIndex { get; set; } // 1 (least popular) to 16 (most popular)
    }
}
