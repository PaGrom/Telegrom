using System;
using System.Threading;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramMessageReceiver : IDisposable
    {
        void Start();
        void SetMessageReceivedHandler(Action<UserMessage, CancellationToken> handler);
        void SetCallbackButtonReceivedHandler(Action<UserCallback, CancellationToken> handler);
    }
}
