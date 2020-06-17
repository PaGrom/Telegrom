using System;
using Telegrom.Core;

namespace ShowMustNotGoOn.Core.Model
{
    public sealed class BotMessage : ISessionAttribute
    {
        public Guid Id { get; set; }

        public BotCommandType BotCommandType { get; set; }

        public Guid MessageTextId { get; set; }

        public TvShowInfo TvShowInfo { get; set; }
    }
}
