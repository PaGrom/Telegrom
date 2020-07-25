using System;

namespace Telegrom.Database
{
    public sealed class DatabaseOptions
    {
        public string ConnectionString { get; }

        private static DatabaseOptions _current;

        public static DatabaseOptions Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }

        public DatabaseOptions(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
