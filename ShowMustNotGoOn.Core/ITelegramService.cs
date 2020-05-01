using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.DatabaseContext.Model;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
	    Task<T> MakeRequestAsync<T>(RequestBase<T> request, CancellationToken cancellationToken);
        Task SendTextMessageToUserAsync(User user, string text, CancellationToken cancellationToken);
        Task<BotMessage> SendMessageToUserAsync(User user, BotMessage message, CancellationToken cancellationToken);
        Task<BotMessage> UpdateMessageAsync(User user, BotMessage message, string callbackId, CancellationToken cancellationToken);
        Task RemoveMessageAsync(User user, BotMessage message, CancellationToken cancellationToken);
    }
}
