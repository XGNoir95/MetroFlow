using Microsoft.EntityFrameworkCore;
using MetroFlow.Models;

namespace MetroFlow.Services
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }
        public DbSet<User> Users{ get; set; }
        public DbSet<Video> Videos{ get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Distress> Distresses { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<LostItem> LostItems { get; set; }

    }
}
