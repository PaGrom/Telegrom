using System;
using System.Threading;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core
{
    public interface ITelegramUpdateReceiver : IDisposable
    {
        void Start();
        void SetUpdateReceivedHandler(Action<Update, CancellationToken> handler);
    }
}
