using Microsoft.EntityFrameworkCore;
using ShowMustNotGoOn.DatabaseContext.Entities;

namespace ShowMustNotGoOn.DatabaseContext
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
