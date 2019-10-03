﻿using System.Collections.Generic;

namespace ShowMustNotGoOn.Core.Model
{
    public sealed class User
    {
        public int Id { get; set; }
        public int TelegramId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<BotMessage> BotMessages { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
