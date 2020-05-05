using System;
using System.Threading;
using ShowMustNotGoOn.Core.TelegramModel;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramUpdateReceiver : IDisposable
    {
        void Start();
        void SetUpdateReceivedHandler(Action<Update, CancellationToken> handler);
    }
}
