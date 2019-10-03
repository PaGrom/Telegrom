using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model.Callback;

namespace ShowMustNotGoOn.Messages.Events
{
    public class TelegramCallbackButtonReceivedEvent : IMessage
    {
        public CallbackButton CallbackButton { get; }

        public TelegramCallbackButtonReceivedEvent(CallbackButton callbackButton)
        {
            CallbackButton = callbackButton;
        }
    }
}
