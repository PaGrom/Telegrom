using System;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramMessageReceiver : IDisposable
    {
        void Start();
        void SetMessageReceivedHandler(Action<UserMessage> handler);
        void SetCallbackButtonReceivedHandler(Action<UserCallback> handler);
    }
}
