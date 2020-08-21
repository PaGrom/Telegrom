using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.MessageBus
{
    public interface IOutgoingRequestQueueReader
    {
        ValueTask<RequestBase> DequeueAsync(CancellationToken cancellationToken);
    }
}
