using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegrom.Core
{
    public interface IWakeUpService
    {
        Task WakeUpAsync(Func<Update, CancellationToken, Task> handler, CancellationToken cancellationToken);
    }
}
