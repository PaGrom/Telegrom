using System;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
        void SetMessageReceivedHandler(Action<Message> handler);
        void Start();
    }
}