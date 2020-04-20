using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
        Task SendTextMessageToUserAsync(User user, string text);
        Task<BotMessage> SendMessageToUserAsync(User user, BotMessage message);
        Task<BotMessage> UpdateMessageAsync(User user, BotMessage message, string callbackId);
        Task RemoveMessageAsync(User user, BotMessage message);
    }
}
