using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegrom.Core.MessageBus
{
    public interface IIncomingUpdateQueueReader
    {
        ValueTask<Update> DequeueAsync(CancellationToken cancellationToken);
    }
}
