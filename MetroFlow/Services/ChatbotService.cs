using System;
using MetroFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace MetroFlow.Services
{
    public class ChatbotService
    {
        private readonly ILogger<ChatbotService> _logger;
        private readonly ApplicationDbContext _context;

        public ChatbotService(ILogger<ChatbotService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ---------------- CRUD ----------------
        public async Task AddChatbotAsync(Chatbot chatbot)
        {
            try
            {
                _context.Chatbots.Add(chatbot);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Chatbot added successfully: {Id}", chatbot.ChatbotId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding chatbot: {Id}", chatbot.ChatbotId);
                throw;
            }
        }

        public async Task<Chatbot> GetChatbotAsync(int id)
        {
            return await _context.Chatbots.FindAsync(id);
        }

        public async Task<List<Chatbot>> GetAllChatbotsAsync()
        {
            return await _context.Chatbots.ToListAsync();
        }

        public async Task<bool> DeleteChatbotAsync(int id)
        {
            var chatbot = await _context.Chatbots.FindAsync(id);
            if (chatbot == null) return false;
            _context.Chatbots.Remove(chatbot);
            await _context.SaveChangesAsync();
            return true;
        }

        // ---------------- Chatbot Logic ----------------
        public Task<string> GetResponseAsync(string message)
        {
            message = message.ToLower();

            if (message.Contains("ticket"))
            {
                return Task.FromResult("Ticket options: Single journey ($1.50-$3.00), Day pass ($8.00), MRT Pass (Tk 500 with 10% discount).");
            }
            else if (message.Contains("schedule") || message.Contains("time"))
            {
                return Task.FromResult("Schedule: First train 5:30 AM, Last train 11:00 PM (Sat-Thu). Friday service starts at 3:00 PM.");
            }
            else if (message.Contains("fare") || message.Contains("price"))
            {
                return Task.FromResult("Fares: $1.50 - $3.00 depending on distance. Students & seniors get 50% discount.");
            }
            else if (message.Contains("lost") || message.Contains("found"))
            {
                return Task.FromResult("Lost & Found: Available at Uttara North & Motijheel stations. Contact: +880-XXXX-LOST.");
            }
            else if (message.Contains("status") || message.Contains("delay"))
            {
                return Task.FromResult("Service Status: North-South line normal ✅, East-West line delayed ⚠️ due to maintenance.");
            }
            else if (message.Contains("contact") || message.Contains("help"))
            {
                return Task.FromResult("Contact MetroFlow: +880-XXXX-XXXXXX | info@metroflow.com | Emergency: +880-XXXX-EMERGENCY.");
            }
            else
            {
                return Task.FromResult("I can help with tickets, fares, schedules, lost items, service status, and contacts. Try asking about one of these!");
            }
        }

        internal async Task<bool> UpdateChatbotAsync(Chatbot chatbot)
        {
            throw new NotImplementedException();
        }
    }
}
