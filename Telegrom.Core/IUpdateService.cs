using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core
{
    public interface IUpdateService
    {
        Task SaveUpdateAsync(Update update, CancellationToken cancellationToken);
        Task MakeUpdateProcessedAsync(Update update, CancellationToken cancellationToken);
    }
}
