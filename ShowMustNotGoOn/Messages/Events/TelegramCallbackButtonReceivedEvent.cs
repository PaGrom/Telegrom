using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Events
{
    public class TelegramCallbackButtonReceivedEvent : IMessage
    {
        public UserCallback UserCallback { get; }

        public int UserId => UserCallback.User.TelegramId;

        public TelegramCallbackButtonReceivedEvent(UserCallback userCallback)
        {
            UserCallback = userCallback;
        }
    }
}
