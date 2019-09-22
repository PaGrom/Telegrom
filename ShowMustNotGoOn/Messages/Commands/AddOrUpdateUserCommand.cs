using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Commands
{
    public class AddOrUpdateUserCommand : IMessage
    {
        public User User { get; }

        public AddOrUpdateUserCommand(User user)
        {
            User = user;
        }
    }
}