using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
	    Task MakeRequestAsync(TelegramModel.Request request, CancellationToken cancellationToken);
        Task SendTextMessageToUserAsync(User user, string text, CancellationToken cancellationToken);
        Task SendMessageToUserAsync(User user, BotMessage message, CancellationToken cancellationToken);
        Task UpdateMessageAsync(User user, BotMessage message, int telegramMessageId, string callbackId, CancellationToken cancellationToken);
        Task RemoveMessageAsync(User user, int messageId, CancellationToken cancellationToken);
    }
}
