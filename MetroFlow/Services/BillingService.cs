using MetroFlow.Models;

namespace MetroFlow.Services
{
    public class BillingService : IBillingService
    {
        // Based on Dhaka Metro Rail fare structure from web research
        private const decimal BASE_FARE = 20.0m; // BDT
        private const decimal FARE_PER_STATION = 5.0m; // BDT per station
        private const decimal MAX_FARE = 100.0m; // BDT
        private const decimal MRT_PASS_DISCOUNT_PERCENTAGE = 0.10m; // 10% discount

        public FareBilling CalculateFare(Station originStation, Station destinationStation, int stationsCount)
        {
            // Ensure we have the correct station count for fare calculation
            // stationsCount should represent the number of stations traveled through
            var actualStationsForFare = Math.Max(stationsCount, 1); // Minimum 1 station for base fare

            var singleJourneyFare = GetStationDistanceFare(actualStationsForFare);
            var mrtPassFare = singleJourneyFare - CalculateMrtPassDiscount(singleJourneyFare);

            return new FareBilling
            {
                DistanceInStations = stationsCount, // This is what shows in the UI
                SingleJourneyFare = singleJourneyFare,
                MrtPassFare = mrtPassFare,
                MrtPassDiscount = MRT_PASS_DISCOUNT_PERCENTAGE * 100, // Convert to percentage
                MrtPassSavings = singleJourneyFare - mrtPassFare,
                OriginStationName = originStation.Name,
                DestinationStationName = destinationStation.Name,
                IsEligibleForMrtPass = true,
                PaymentMethods = GetPaymentMethods(singleJourneyFare, mrtPassFare)
            };
        }

        public decimal GetStationDistanceFare(int stationCount)
        {
            if (stationCount <= 0) return BASE_FARE;

            // Dhaka Metro Rail fare structure:
            // Base fare ৳20 + ৳5 per additional station beyond the first
            // For example: 1 station = ৳20, 2 stations = ৳25, 3 stations = ৳30, etc.
            var calculatedFare = BASE_FARE + ((stationCount - 1) * FARE_PER_STATION);
            return Math.Min(calculatedFare, MAX_FARE);
        }

        public decimal CalculateMrtPassDiscount(decimal baseFare)
        {
            return Math.Round(baseFare * MRT_PASS_DISCOUNT_PERCENTAGE, 2);
        }

        public List<PaymentMethod> GetPaymentMethods(decimal singleJourneyFare, decimal mrtPassFare)
        {
            return new List<PaymentMethod>
            {
                new PaymentMethod
                {
                    Type = "SingleJourney",
                    Title = "Single Journey Ticket",
                    Description = "Perfect for occasional travel",
                    Amount = singleJourneyFare,
                    IconClass = "fas fa-ticket-alt",
                    IsRecommended = false,
                    Benefits = new List<string>
                    {
                        "No registration required",
                        "Buy from TVM or ticket counter",
                        "Valid for one journey only",
                        "Pay as you travel",
                        "Special discount for disabled(15 %)"
                    }
                },
                new PaymentMethod
                {
                    Type = "MRTPass",
                    Title = "MRT Pass (Smart Card)",
                    Description = "Best value for regular commuters",
                    Amount = mrtPassFare,
                    IconClass = "fas fa-credit-card",
                    IsRecommended = true,
                    Benefits = new List<string>
                    {
                        "10% discount on every trip",
                        "No queuing for tickets",
                        "10 years validity",
                        "Top up to ৳10,000 balance",
                        "Contactless NFC technology"
                    }
                }
            };
        }
    }
}
