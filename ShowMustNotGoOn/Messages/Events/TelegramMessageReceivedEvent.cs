using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Events
{
    public sealed class TelegramMessageReceivedEvent : IMessage
    {
        public Message Message { get; }

        public TelegramMessageReceivedEvent(Message message)
        {
            Message = message;
        }
    }
}
