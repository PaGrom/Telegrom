using System;
using Microsoft.EntityFrameworkCore;
using Telegrom.Core.Configuration;

namespace Telegrom.Database.InMemory
{
    public static class TelegromConfigurationExtensions
    {
        public static ITelegromConfiguration UseInMemoryDatabase(
            this ITelegromConfiguration configuration,
            string databaseName,
            Action<DbContextOptionsBuilder> action)
        {
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName);

            action(optionsBuilder);

            return configuration.Use(DatabaseOptions.Current, x => DatabaseOptions.Current = optionsBuilder);
        }
    }
}
