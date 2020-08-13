using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegrom.Core.Contexts;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;

namespace Telegrom
{
    internal sealed class UpdateDispatcher : IUpdateDispatcher
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger<UpdateDispatcher> _logger;
        private readonly IGlobalIncomingUpdateQueueReader _incomingUpdateQueueReader;
        private readonly IGlobalIncomingUpdateQueueWriter _incomingUpdateQueueWriter;

        public UpdateDispatcher(SessionManager sessionManager,
            IGlobalIncomingUpdateQueueWriter incomingUpdateQueueWriter,
            IGlobalIncomingUpdateQueueReader incomingUpdateQueueReader,
            ILogger<UpdateDispatcher> logger)
        {
            _sessionManager = sessionManager;
            _incomingUpdateQueueWriter = incomingUpdateQueueWriter;
            _incomingUpdateQueueReader = incomingUpdateQueueReader;
            _logger = logger;
        }

        public async Task DispatchAsync(Update update, CancellationToken cancellationToken)
        {
            await _incomingUpdateQueueWriter.EnqueueAsync(update, cancellationToken);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting update dispatcher");
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    var update = await _incomingUpdateQueueReader.DequeueAsync(cancellationToken);

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
