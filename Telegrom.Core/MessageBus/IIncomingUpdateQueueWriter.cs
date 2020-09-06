using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegrom.Core.MessageBus
{
    public interface IIncomingUpdateQueueWriter
    {
        ValueTask EnqueueAsync(Update update, CancellationToken cancellationToken);
        void Complete();
    }
}
