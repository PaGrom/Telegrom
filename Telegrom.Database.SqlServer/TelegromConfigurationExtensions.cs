using System;
using Microsoft.EntityFrameworkCore;
using Telegrom.Core.Configuration;

namespace Telegrom.Database.SqlServer
{
    public static class TelegromConfigurationExtensions
    {
        public static ITelegromConfiguration UseSqlServerDatabase(
            this ITelegromConfiguration configuration,
            string connectionString,
            Action<DbContextOptionsBuilder> action)
        {
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlServer(connectionString);

            action(optionsBuilder);

            return configuration.Use(DatabaseOptions.Current, x => DatabaseOptions.Current = optionsBuilder);
        }
    }
}
