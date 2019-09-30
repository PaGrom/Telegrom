using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model.CallbackQuery;

namespace ShowMustNotGoOn.Messages.Events
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
