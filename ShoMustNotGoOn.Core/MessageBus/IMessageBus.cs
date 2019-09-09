using System;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public interface IMessageBus
    {
        void RegisterHandler<T>(Action<T> handleAction) where T : IJob;
        Task Enqueue(IJob job);
        void Stop();
    }
}
