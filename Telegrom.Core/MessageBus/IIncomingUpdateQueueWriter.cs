using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.MessageBus
{
    public interface IIncomingUpdateQueueWriter
    {
        ValueTask EnqueueAsync(Update update, CancellationToken cancellationToken);
    }
}
