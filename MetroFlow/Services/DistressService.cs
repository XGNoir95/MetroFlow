using System;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MetroFlow.Models;

namespace MetroFlow.Services
{
    public class DistressService
    {
        private readonly ILogger<DistressService> _logger;
        private readonly ApplicationDbContext _context;

        public DistressService(ILogger<DistressService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Add a new distress report
        public async Task AddDistressAsync(Distress distress)
        {
            try
            {
                _context.Distresses.Add(distress);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Distress added successfully: {Id}, Type: {Type}, Station: {Station}",
                    distress.DistressId, distress.EmergencyType, distress.StationName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding distress report");
                throw;
            }
        }

        // Get single distress (including user if applicable)
        public async Task<Distress?> GetDistressAsync(int id)
        {
            return await _context.Distresses
                                 .Include(d => d.User) // only works if User navigation exists
                                 .FirstOrDefaultAsync(d => d.DistressId == id);
        }

        // Get all distress reports
        public async Task<List<Distress>> GetAllDistressesAsync()
        {
            return await _context.Distresses
                                 .OrderByDescending(d => d.ReportedAt)
                                 .ToListAsync();
        }

        // Filtered queries
        public async Task<List<Distress>> GetDistressesByStatusAsync(string status)
        {
            return await _context.Distresses
                                 .Where(d => d.Status == status)
                                 .OrderByDescending(d => d.ReportedAt)
                                 .ToListAsync();
        }

        public async Task<List<Distress>> GetDistressesByTypeAsync(string emergencyType)
        {
            return await _context.Distresses
                                 .Where(d => d.EmergencyType == emergencyType)
                                 .OrderByDescending(d => d.ReportedAt)
                                 .ToListAsync();
        }

        public async Task<List<Distress>> GetDistressesByStationAsync(string stationName)
        {
            return await _context.Distresses
                                 .Where(d => d.StationName == stationName)
                                 .OrderByDescending(d => d.ReportedAt)
                                 .ToListAsync();
        }

        // Update an existing distress (e.g., status, severity, or description)
        public async Task<bool> UpdateDistressAsync(Distress updated)
        {
            var distress = await _context.Distresses.FindAsync(updated.DistressId);
            if (distress == null) return false;

            distress.Status = updated.Status ?? distress.Status;
            distress.Description = updated.Description ?? distress.Description;
            distress.Severity = updated.Severity ?? distress.Severity;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Distress updated: {Id}, New Status: {Status}", distress.DistressId, distress.Status);
            return true;
        }

        internal async Task<bool> DeleteDistressAsync(int id)
        {
            throw new NotImplementedException();
        }

        // Delete distress report
        public
    }
