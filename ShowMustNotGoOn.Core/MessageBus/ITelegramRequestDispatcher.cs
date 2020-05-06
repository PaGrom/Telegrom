using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public interface ITelegramRequestDispatcher
    {
        Task DispatchAsync(Request request, CancellationToken cancellationToken);
        Task RunAsync(CancellationToken cancellationToken);
    }
}
