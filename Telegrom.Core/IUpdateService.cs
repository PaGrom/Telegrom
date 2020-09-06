using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegrom.Core
{
    public interface IUpdateService
    {
        Task SaveUpdateAsync(Update update, CancellationToken cancellationToken);
        Task MakeUpdateProcessedAsync(Update update, CancellationToken cancellationToken);
    }
}
