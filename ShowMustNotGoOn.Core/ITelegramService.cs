using System;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.Callback;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
        void SetMessageReceivedHandler(Action<Message> handler);
        void SetCallbackQueryReceivedHandler(Action<CallbackQuery> handler);
        void Start();
        Task SendTextMessageToUser(User user, string text);
        Task<Message> SendTvShowToUserAsync(User user, TvShow show,
            ButtonCallbackQueryData nextNavigateCallbackQueryData,
            ButtonCallbackQueryData subscribeEndOfShowCallbackQueryData);
        Task<Message> UpdateTvShowMessageAsync(User user, TvShow show,
            CallbackQuery callbackQuery,
            ButtonCallbackQueryData prevNavigateCallbackQueryData,
            ButtonCallbackQueryData nextNavigateCallbackQueryData,
            ButtonCallbackQueryData subscribeEndOfShowCallbackQueryData);
    }
}
