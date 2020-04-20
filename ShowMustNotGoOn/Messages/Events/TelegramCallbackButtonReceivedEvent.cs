using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Events
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
