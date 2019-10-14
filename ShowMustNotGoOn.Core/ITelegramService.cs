using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
        Task SendTextMessageToUserAsync(User user, string text);
        Task SendMessageToUserAsync(User user, BotMessage message);
        Task UpdateMessageAsync(BotMessage message, string callbackId);
        Task RemoveMessageAsync(BotMessage message);
    }
}
