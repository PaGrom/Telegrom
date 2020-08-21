using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.MessageBus
{
    public interface IRequestDispatcher
    {
        Task DispatchAsync(Request request, CancellationToken cancellationToken);
        Task RunAsync(CancellationToken cancellationToken);
    }
}
