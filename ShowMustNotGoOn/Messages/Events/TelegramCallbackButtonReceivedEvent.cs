using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model.Callback;

namespace ShowMustNotGoOn.Messages.Events
{
    public class TelegramCallbackButtonReceivedEvent : IMessage
    {
        public CallbackButton CallbackButton { get; }

        public int UserId => CallbackButton.Message.User.TelegramId;

        public TelegramCallbackButtonReceivedEvent(CallbackButton callbackButton)
        {
            CallbackButton = callbackButton;
        }
    }
}
