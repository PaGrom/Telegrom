using System;
using Telegrom.Core;

namespace ShowMustNotGoOn.Core.Model
{
    public sealed class Callback : ISessionAttribute
    {
        public Guid Id { get; set; }

        public Guid BotMessageId { get; set; }

        public CallbackType CallbackType { get; set; }
    }
}
