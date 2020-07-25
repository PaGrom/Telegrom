using System;

namespace Telegrom.TelegramService
{
    public sealed class TelegramOptions
    {
        public string TelegramApiToken { get; set; }
        public string ProxyAddress { get; set; }
        public string Socks5HostName { get; set; }
        public int? Socks5Port { get; set; }
        public string Socks5Username { get; set; }
        public string Socks5Password { get; set; }

        private static TelegramOptions _current;

        public static TelegramOptions Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
