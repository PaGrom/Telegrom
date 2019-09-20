using ShowMustNotGoOn.Core.MessageBus;
using Telegram.Bot.Types;

namespace ShowMustNotGoOn.Messages.Event
{
    public sealed class TelegramMessageReceivedEvent : IMessage
    {
        private readonly Message _message;

        public TelegramMessageReceivedEvent(Message message)
        {
            _message = message;
        }
    }
}
