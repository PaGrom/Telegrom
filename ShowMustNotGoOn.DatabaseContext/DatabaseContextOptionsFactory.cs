using Microsoft.EntityFrameworkCore;

namespace ShowMustNotGoOn.DatabaseContext
{
    public class DatabaseContextOptionsFactory
    {
        public static DbContextOptions<DatabaseContext> Get(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseSqlite(connectionString);

            return builder.Options;
        }
    }
}
