using System;
using Microsoft.Extensions.Logging;

namespace Telegrom
{
    public sealed class LoggerOptions
    {
        private static ILoggerFactory _current;

        public static ILoggerFactory Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
