using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core.MessageBus.Events
{
    public class TelegramCallbackButtonReceivedEvent : IMessage
    {
        public UserCallback UserCallback { get; }

        public User User => UserCallback.User;

        public TelegramCallbackButtonReceivedEvent(UserCallback userCallback)
        {
            UserCallback = userCallback;
        }
    }
}
