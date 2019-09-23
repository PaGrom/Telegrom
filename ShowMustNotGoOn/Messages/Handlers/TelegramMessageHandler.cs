using System;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Messages.Commands;
using ShowMustNotGoOn.Messages.Event;

namespace ShowMustNotGoOn.Messages.Handlers
{
    public sealed class TelegramMessageHandler : IDisposable
    {
        private readonly ITelegramService _telegramService;
        private readonly IMessageBus _messageBus;
        private readonly ILogger _logger;

        public TelegramMessageHandler(ITelegramService telegramService,
            IMessageBus messageBus,
            ILogger logger)
        {
            _telegramService = telegramService;
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
            _messageBus.RegisterHandler<TelegramMessageReceivedEvent>(HandleTelegramMessageReceivedEvent);
            _messageBus.RegisterHandler<SendWelcomeMessageToUserCommand>(HandleSendWelcomeMessageToUserCommand);
        }

        private void UnregisterHandlers()
        {
            _messageBus.UnregisterHandler<TelegramMessageReceivedEvent>(HandleTelegramMessageReceivedEvent);
            _messageBus.UnregisterHandler<SendWelcomeMessageToUserCommand>(HandleSendWelcomeMessageToUserCommand);
        }

        private async Task HandleTelegramMessageReceivedEvent(TelegramMessageReceivedEvent @event)
        {
            await _messageBus.Enqueue(new AddOrUpdateUserCommand(@event.Message.FromUser));
            if (@event.Message.BotCommand == BotCommandType.Start)
            {
                await _messageBus.Enqueue(new SendWelcomeMessageToUserCommand(@event.Message.FromUser));
            }
        }

        private async Task HandleSendWelcomeMessageToUserCommand(SendWelcomeMessageToUserCommand command)
        {
            await _telegramService.SendWelcomeMessageToUser(command.User);
        }
    }
}
