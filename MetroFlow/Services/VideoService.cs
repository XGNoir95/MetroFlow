using System;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MetroFlow.Models;


namespace MetroFlow.Services
{
    public class VideoService
    {
        private readonly ILogger<VideoService> _logger;
        private readonly ApplicationDbContext _context;
        public VideoService(ILogger<VideoService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        // Add video to database
        public async Task AddVideoAsync(Video video)
        {
            try
            {
                _context.Videos.Add(video);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Video added successfully: {Title}", video.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding video: {Title}", video.Title);
                throw;
            }
        }
        public async Task<Video?> GetVideoAsync(int id)
        {
            return await _context.Videos.FindAsync(id);
        }
        public async Task<List<Video>> GetAllVideosAsync()
        {
            return await _context.Videos.ToListAsync();
        }
        public async Task<bool> DeleteVideoAsync(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null) return false;

            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
