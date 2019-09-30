using System;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.CallbackQuery;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
        void SetMessageReceivedHandler(Action<Message> handler);
        void SetCallbackQueryReceivedHandler(Action<CallbackQuery> handler);
        void Start();
        Task SendTextMessageToUser(User user, string text);
        Task<Message> SendTvShowToUser(User user, TvShow show,
            int? nextNavigateCallbackQueryDataId);
        Task<Message> UpdateTvShowMessage(User user, TvShow show,
            CallbackQuery callbackQuery,
            int? prevNavigateCallbackQueryDataId,
            int? nextNavigateCallbackQueryDataId);
    }
}