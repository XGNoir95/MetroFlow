using Microsoft.EntityFrameworkCore;
using MetroFlow.Models;

namespace MetroFlow.Services
{
    public class LostAndFoundService
    {
        private readonly ApplicationDbContext _context;

        public LostAndFoundService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LostItem>> GetAllLostItemsAsync()
        {
            return await _context.LostItems.ToListAsync();
        }

        public async Task<LostItem> UpdateLostItemStatusAsync(int lostItemId, bool isClaimed)
        {
            var lostItem = await _context.LostItems.FindAsync(lostItemId);
            if (lostItem == null)
            {
                throw new KeyNotFoundException($"Lost item with ID {lostItemId} not found.");
            }

            lostItem.IsClaimed = isClaimed;
            await _context.SaveChangesAsync();
            return lostItem;
        }

        public async Task<bool> DeleteLostItemAsync(int lostItemId)
        {
            var lostItem = await _context.LostItems.FindAsync(lostItemId);
            if (lostItem == null)
            {
                return false;
            }

            _context.LostItems.Remove(lostItem);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}