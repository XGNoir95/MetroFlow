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
    }
}
