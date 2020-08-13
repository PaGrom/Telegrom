using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.MessageBus
{
    public interface IUpdateDispatcher
    {
        Task DispatchAsync(Update update, CancellationToken cancellationToken);
        Task RunAsync(CancellationToken cancellationToken);
    }
}
