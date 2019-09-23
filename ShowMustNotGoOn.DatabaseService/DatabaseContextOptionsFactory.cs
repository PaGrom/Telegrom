using Microsoft.EntityFrameworkCore;
using ShowMustNotGoOn.DatabaseService.Entities;

namespace ShowMustNotGoOn.DatabaseService
{
    public class DatabaseContextOptionsFactory
    {
        public static DbContextOptions<ShowsDbContext> Get(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<ShowsDbContext>();
            builder.UseSqlite(connectionString);

            return builder.Options;
        }
    }
}
