using MetroFlow.Models;

namespace MetroFlow.Services
{
    public interface IHeatmapService
    {
        object GetStationHeatmapData(Station station, string currentPeriod);
        List<object> GetAllStationsHeatmapData(string currentPeriod);
        List<object> GetTimePeriodOptions();
        string GetPeriodTypeFromTimeRange(string timeRange, string dayType);
    }
}
