using System;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages.Commands;

namespace ShowMustNotGoOn.Messages.Handlers
{
    public sealed class UsersMessageHandler : IDisposable
    {
        private readonly IUsersService _usersService;
        private readonly IMessageBus _messageBus;
        private readonly ILogger _logger;

        public UsersMessageHandler(IUsersService usersService,
            IMessageBus messageBus,
            ILogger logger)
        {
            _usersService = usersService;
            _messageBus = messageBus;
            _logger = logger;

            RegisterHandlers();
        }

        public void Dispose()
        {
            UnregisterHandlers();
        }

        private void RegisterHandlers()
        {
            _messageBus.RegisterHandler<AddOrUpdateUserCommand>(HandleAddOrUpdateUserCommand);
        }

        private void UnregisterHandlers()
        {
            _messageBus.UnregisterHandler<AddOrUpdateUserCommand>(HandleAddOrUpdateUserCommand);
        }

        private async Task HandleAddOrUpdateUserCommand(AddOrUpdateUserCommand command)
        {
            await _usersService.AddOrUpdateUserAsync(command.User);
        }
    }
}
