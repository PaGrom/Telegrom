using System;
using Microsoft.EntityFrameworkCore;

namespace Telegrom.Database
{
    public sealed class DatabaseOptions
    {
        private static DbContextOptionsBuilder<DatabaseContext> _current;

        public static DbContextOptionsBuilder<DatabaseContext> Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
