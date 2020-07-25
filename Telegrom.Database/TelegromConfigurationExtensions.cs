using System;
using Telegrom.Core.Configuration;

namespace Telegrom.Database
{
    public static class TelegromConfigurationExtensions
    {
        public static ITelegromConfiguration AddDatabaseOptions(
            this ITelegromConfiguration configuration,
            DatabaseOptions databaseOptions)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (databaseOptions == null) throw new ArgumentNullException(nameof(databaseOptions));

            return configuration.Use(databaseOptions, x => DatabaseOptions.Current = x);
        }
    }
}
