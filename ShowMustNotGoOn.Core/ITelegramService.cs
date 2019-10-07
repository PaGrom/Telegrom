using System;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.Callback;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
        void SetMessageReceivedHandler(Action<UserMessage> handler);
        void SetCallbackButtonReceivedHandler(Action<CallbackButton> handler);
        void Start();
        Task SendTextMessageToUserAsync(User user, string text);
        Task SendMessageToUserAsync(User user, BotMessage message);
        Task UpdateMessageAsync(BotMessage message, string callbackId);
        Task RemoveMessageAsync(BotMessage message);
    }
}
