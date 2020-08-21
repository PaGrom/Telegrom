using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.MessageBus
{
    public interface IRequestDispatcher
    {
        Task DispatchAsync(RequestBase requestBase, CancellationToken cancellationToken);
        Task RunAsync(CancellationToken cancellationToken);
    }
}
