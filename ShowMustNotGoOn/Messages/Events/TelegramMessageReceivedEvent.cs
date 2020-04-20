using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Events
{
    public sealed class TelegramMessageReceivedEvent : IMessage
    {
        public UserMessage UserMessage { get; }

        public User User => UserMessage.User;

        public TelegramMessageReceivedEvent(UserMessage userMessage)
        {
            UserMessage = userMessage;
        }
    }
}
