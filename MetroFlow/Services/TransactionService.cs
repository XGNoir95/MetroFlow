using MetroFlow.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
namespace MetroFlow.Services
{
    public class TransactionService
    {
        private readonly ILogger<TransactionService> _logger;
        private readonly ApplicationDbContext _context;
        public TransactionService(ILogger<TransactionService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        // Add transaction to database
        public async Task AddTransactionAsync(Transaction transaction)
        {
            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Transaction added successfully: {Id}", transaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding transaction: {Id}", transaction.TransactionId);
                throw;
            }
        }
        public async Task<Transaction?> GetTransactionAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }
        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions.ToListAsync();
        }
        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
