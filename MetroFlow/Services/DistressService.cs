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
        public async Task AddDistressAsync(Distress distress)
        {
            try
            {
                _context.Distresses.Add(distress);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Distress added successfully: {Id}", distress.DistressId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding distress: {Id}", distress.DistressId);
                throw;
            }
        }
        public async Task<Distress?> GetDistressAsync(int id)
        {
            return await _context.Distresses.FindAsync(id);
        }
        public async Task<List<Distress>> GetAllDistressesAsync()
        {
            return await _context.Distresses.ToListAsync();
        }
        public async Task<bool> DeleteDistressAsync(int id)
        {
            var distress = await _context.Distresses.FindAsync(id);
            if (distress == null) return false;
            _context.Distresses.Remove(distress);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
