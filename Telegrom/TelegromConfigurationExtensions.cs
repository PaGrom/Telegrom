using System;
using Microsoft.Extensions.Logging;
using Telegrom.Core.Configuration;

namespace Telegrom
{
    public static class TelegromConfigurationExtensions
    {
        public static ITelegromConfiguration UseLoggerFactory(
            this ITelegromConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            return configuration.Use(LoggerOptions.Current, x => LoggerOptions.Current = loggerFactory);
        }
    }
}
