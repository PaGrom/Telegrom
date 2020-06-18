using System;
using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core
{
    public interface IWakeUpService
    {
        Task WakeUpAsync(Action<Update, CancellationToken> handler, CancellationToken cancellationToken);
    }
}
