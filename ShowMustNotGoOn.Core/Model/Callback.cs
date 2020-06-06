using System;

namespace ShowMustNotGoOn.Core.Model
{
    public sealed class Callback
    {
        public Guid Id { get; set; }

        public Guid BotMessageId { get; set; }

        public CallbackType CallbackType { get; set; }
    }
}
