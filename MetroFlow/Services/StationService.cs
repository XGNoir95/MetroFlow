using System;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MetroFlow.Models;

namespace MetroFlow.Services
{
    public class StationService
    {
        private readonly ILogger<StationService> _logger;
        private readonly ApplicationDbContext _context;
        public StationService(ILogger<StationService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task AddStationAsync(Station station)
        {
            try
            {
                _context.Stations.Add(station);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Station added successfully: {Name}", station.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding station: {Name}", station.Name);
                throw;
            }
        }
        public async Task<Station?> GetStationAsync(int id)
        {
            return await _context.Stations.FindAsync(id);
        }
        public async Task<List<Station>> GetAllStationsAsync()
        {
            return await _context.Stations.ToListAsync();
        }
        public async Task<bool> DeleteStationAsync(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station == null) return false;
            _context.Stations.Remove(station);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
