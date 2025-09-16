using BCrypt.Net;
using MetroFlow.Models;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace MetroFlow.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Admin?> GetAdminByUsernameAsync(string username)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.Username == username);
        }

        public async Task<Admin?> GetAdminByEmailAsync(string email)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Admin?> ValidateAdminCredentialsAsync(string username, string password)
        {
            var admin = await GetAdminByUsernameAsync(username);

            if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            {
                return admin;
            }

            return null;
        }
        public async Task<List<Admin>> GetAllAdminsAsync()
        {
            return await _context.Admins.ToListAsync();
        }
        public async Task<Admin?> GetAdminByIdAsync(int adminId)
        {
            return await _context.Admins.FindAsync(adminId);
        }
        public async Task<Admin> CreateAdminAsync(Admin admin, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            }

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            admin.CreatedAt = DateTime.UtcNow;

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<Admin> UpdateAdminAsync(int adminId, Admin updatedAdmin, string? newPassword = null)
        {
            var existingAdmin = await _context.Admins.FindAsync(adminId);
            if (existingAdmin == null)
            {
                throw new KeyNotFoundException($"Admin with ID {adminId} not found.");
            }

            existingAdmin.Username = updatedAdmin.Username;
            existingAdmin.Email = updatedAdmin.Email;

            // Update password only if a new one is provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                existingAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            existingAdmin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingAdmin;
        }

        public async Task DeleteAdminAsync(int adminId)
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null)
            {
                throw new KeyNotFoundException($"Admin with ID {adminId} not found.");
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
        }
    }
}