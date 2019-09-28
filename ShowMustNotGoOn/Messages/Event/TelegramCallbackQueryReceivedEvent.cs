using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Event
{
    public class TelegramCallbackQueryReceivedEvent : IMessage
    {
        public CallbackQuery CallbackQuery { get; }

        public TelegramCallbackQueryReceivedEvent(CallbackQuery callbackQuery)
        {
            CallbackQuery = callbackQuery;
        }
    }
}
