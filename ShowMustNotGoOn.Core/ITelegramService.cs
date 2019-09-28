using System;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
        void SetMessageReceivedHandler(Action<Message> handler);
        void SetCallbackQueryReceivedHandler(Action<CallbackQuery> handler);
        void Start();
        Task SendTextMessageToUser(User user, string text);
        Task SendTvShowToUser(User user, TvShow show, string nextCallbackQueryData);
        Task UpdateTvShowMessage(User user, TvShow show, int messageId, string nextCallbackQueryData, string prevCallbackQueryData);
    }
}