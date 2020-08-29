using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;

namespace Telegrom
{
    internal sealed class RequestDispatcher : IRequestDispatcher
    {
        private readonly ILogger<RequestDispatcher> _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IGlobalOutgoingRequestQueueReader _outgoingRequestQueueReader;
        private readonly IGlobalOutgoingRequestQueueWriter _outgoingRequestQueueWriter;

        public RequestDispatcher(
            ITelegramBotClient telegramBotClient,
            IGlobalOutgoingRequestQueueReader outgoingRequestQueueReader,
            IGlobalOutgoingRequestQueueWriter outgoingRequestQueueWriter,
            ILogger<RequestDispatcher> logger)
        {
            _telegramBotClient = telegramBotClient;
            _outgoingRequestQueueWriter = outgoingRequestQueueWriter;
            _outgoingRequestQueueReader = outgoingRequestQueueReader;
            _logger = logger;
        }

        public async Task DispatchAsync(Request request, CancellationToken cancellationToken)
        {
            await _outgoingRequestQueueWriter.EnqueueAsync(request, cancellationToken);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting request dispatcher");
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    var request = await _outgoingRequestQueueReader.DequeueAsync(cancellationToken);

                    await MakeRequestAsync(request, cancellationToken);
                    await Task.Delay(33, cancellationToken);
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

            _logger.LogInformation("Request dispatcher cancelled");
        }

        private async Task MakeRequestAsync(Request request, CancellationToken cancellationToken)
        {
            var respondType = request.GenericArgumentType;

            var makeRequestAsyncMethodInfo = _telegramBotClient.GetType().GetMethod(nameof(_telegramBotClient.MakeRequestAsync)).MakeGenericMethod(respondType);

            dynamic awaitable = makeRequestAsyncMethodInfo.Invoke(_telegramBotClient, new []{ request.Instance, cancellationToken });

            await awaitable;

            await request.Callback(awaitable.GetAwaiter().GetResult());
        }
    }
}
