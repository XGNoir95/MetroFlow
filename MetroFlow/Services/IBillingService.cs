using MetroFlow.Models;

namespace MetroFlow.Services
{
    public interface IBillingService
    {
        FareBilling CalculateFare(Station originStation, Station destinationStation, int stationsCount);
        List<PaymentMethod> GetPaymentMethods(decimal singleJourneyFare, decimal mrtPassFare);
        decimal GetStationDistanceFare(int stationCount);
        decimal CalculateMrtPassDiscount(decimal baseFare);
    }
}
