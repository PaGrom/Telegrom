using System;

namespace ShowMustNotGoOn.Core.Model
{
    public sealed class BotMessage
    {
        public Guid Id { get; set; }

        public BotCommandType BotCommandType { get; set; }

        public Guid MessageTextId { get; set; }

        public int MyShowsId { get; set; }
    }
}
