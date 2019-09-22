using System;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
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
        }

        private void UnregisterHandlers()
        {
            _messageBus.UnregisterHandler<TelegramMessageReceivedEvent>(HandleTelegramMessageReceivedEvent);
        }

        private async void HandleTelegramMessageReceivedEvent(TelegramMessageReceivedEvent @event)
        {
            await _messageBus.Enqueue(new AddOrUpdateUserCommand(@event.Message.FromUser));
        }
    }
}
