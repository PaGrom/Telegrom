using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Event
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
