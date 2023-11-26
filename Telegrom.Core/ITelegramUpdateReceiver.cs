using System;
using System.Threading;
using Telegram.Bot.Types;

namespace Telegrom.Core
{
    public interface ITelegramUpdateReceiver
    {
        void Start(CancellationToken cancellationToken);
        void SetUpdateReceivedHandler(Func<Update, CancellationToken, Task> handler);
    }
}
