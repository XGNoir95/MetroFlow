namespace MetroFlow.Models
{
    public class FareBilling
    {
        public int DistanceInStations { get; set; }
        public decimal SingleJourneyFare { get; set; }
        public decimal MrtPassFare { get; set; }
        public decimal MrtPassDiscount { get; set; }
        public decimal MrtPassSavings { get; set; }
        public string OriginStationName { get; set; } = string.Empty;
        public string DestinationStationName { get; set; } = string.Empty;
        public bool IsEligibleForMrtPass { get; set; } = true;
        public List<PaymentMethod> PaymentMethods { get; set; } = new();
    }

    public class PaymentMethod
    {
        public string Type { get; set; } = string.Empty; // "SingleJourney", "MRTPass"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string IconClass { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public List<string> Benefits { get; set; } = new();
    }
}
