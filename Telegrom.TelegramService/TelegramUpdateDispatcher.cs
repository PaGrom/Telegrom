using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegrom.Core.Contexts;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;

namespace Telegrom.TelegramService
{
    internal sealed class TelegramUpdateDispatcher : ITelegramUpdateDispatcher
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger<TelegramUpdateDispatcher> _logger;
        private readonly IChannelReaderProvider<Update> _incomingUpdatesChannelReader;
        private readonly IChannelWriterProvider<Update> _incomingUpdatesChannelWriter;

        public TelegramUpdateDispatcher(SessionManager sessionManager, ILogger<TelegramUpdateDispatcher> logger)
        {
            var channelHolder = new ChannelHolder<Update>();
            _incomingUpdatesChannelReader = channelHolder;
            _incomingUpdatesChannelWriter = channelHolder;
            _sessionManager = sessionManager;
            _logger = logger;
        }

        public async Task DispatchAsync(Update update, CancellationToken cancellationToken)
        {
            await _incomingUpdatesChannelWriter.Writer.WriteAsync(update, cancellationToken);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting update dispatcher");
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    var update = await _incomingUpdatesChannelReader.Reader.ReadAsync(cancellationToken);

                    var sessionContext = await _sessionManager.GetSessionContextAsync(update.From, cancellationToken);
                    await sessionContext.PostUpdateAsync(update, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // discard
                }
                catch (ApiRequestException apiRequestException)
                {
                    _logger.LogError(apiRequestException, string.Empty);

                    // discard api fails
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            await Task.Delay(1000, cancellationToken); // wait for some time and repeat
                        }
                        catch (OperationCanceledException)
                        {
                            // discard
                        }
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, string.Empty);
                    throw;
                }

            _logger.LogInformation("Update dispatcher cancelled");
        }
    }
}
