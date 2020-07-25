using System;
using Telegrom.Core.Configuration;

namespace Telegrom.TelegramService
{
    public static class TelegromConfigurationExtensions
    {
        public static ITelegromConfiguration AddTelegramOptions(
            this ITelegromConfiguration configuration,
            TelegramOptions telegramOptions)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (telegramOptions == null) throw new ArgumentNullException(nameof(telegramOptions));

            return configuration.Use(telegramOptions, x => TelegramOptions.Current = x);
        }
    }
}
