using Microsoft.EntityFrameworkCore;
using WebAPICachingWithRedis.Models;

namespace WebAPICachingWithRedis.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public DbSet<Driver> Drivers { get; set; }
    }
}
