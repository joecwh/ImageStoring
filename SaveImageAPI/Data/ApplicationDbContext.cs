using Microsoft.EntityFrameworkCore;
using SaveImageAPI.Models;

namespace SaveImageAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }
    }
}
