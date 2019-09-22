using System;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public interface IMessageBus
    {
        void RegisterHandler<T>(Func<T, Task> handleAction) where T : IMessage;
        void UnregisterHandler<T>(Func<T, Task> handleAction) where T : IMessage;
        Task Enqueue(IMessage message);
        void Stop();
    }
}
