using System;
using Telegrom.Core;

namespace ShowMustNotGoOn.Core.Model
{
    public sealed class Subscription : ISessionAttribute
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public int TvShowId { get; set; }
    }
}
