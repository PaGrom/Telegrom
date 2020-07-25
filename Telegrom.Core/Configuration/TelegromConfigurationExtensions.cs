using System;

namespace Telegrom.Core.Configuration
{
    public static class TelegromConfigurationExtensions
    {
        public static ITelegromConfiguration<T> Use<T>(
            this ITelegromConfiguration configuration,
            T entry,
            Action<T> entryAction)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            entryAction(entry);

            return new ConfigurationEntry<T>(entry);
        }

        private class ConfigurationEntry<T> : ITelegromConfiguration<T>
        {
            public ConfigurationEntry(T entry)
            {
                Entry = entry;
            }

            public T Entry { get; }
        }
    }
}
