using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core.MessageBus.Events
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
