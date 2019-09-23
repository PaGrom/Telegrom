using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Commands
{
    public sealed class SendWelcomeMessageToUserCommand : IMessage
    {
        public User User { get; }

        public SendWelcomeMessageToUserCommand(User user)
        {
            User = user;
        }
    }
}
