using Microsoft.EntityFrameworkCore;

namespace DbRepository
{
    public class DbContextOptionsFactory
    {
        public static DbContextOptions<ShowsDbContext> Get(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<ShowsDbContext>();
            builder.UseSqlite(connectionString);

            return builder.Options;
        }
    }
}
