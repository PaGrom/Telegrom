using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Events
{
    public sealed class TelegramMessageReceivedEvent : IMessage
    {
        public UserMessage UserMessage { get; }

        public int UserId => UserMessage.User.TelegramId;

        public TelegramMessageReceivedEvent(UserMessage userMessage)
        {
            UserMessage = userMessage;
        }
    }
}
