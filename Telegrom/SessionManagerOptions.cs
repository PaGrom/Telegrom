using System;

namespace Telegrom
{
    public sealed class SessionManagerOptions
    {
        private static int _maxActiveSessionsNumber = 10;

        public static int MaxActiveSessionsNumber
        {
            get => _maxActiveSessionsNumber;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Max active sessions number cannot be less 1");
                }

                _maxActiveSessionsNumber = value;
            }
        }
    }
}
