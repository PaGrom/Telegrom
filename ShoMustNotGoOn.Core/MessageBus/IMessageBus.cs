using System;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public interface IMessageBus
    {
        void RegisterHandler<T>(Action<T> handleAction) where T : IMessage;
        Task Enqueue(IMessage message);
        void Stop();
    }
}
