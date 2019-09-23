using System;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITelegramService
    {
        void SetMessageReceivedHandler(Action<Message> handler);
        void Start();
        Task SendWelcomeMessageToUser(User user);
    }
}