using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public RequestDispatcher(
            ITelegramBotClient telegramBotClient,
            IGlobalOutgoingRequestQueueReader outgoingRequestQueueReader,
            IGlobalOutgoingRequestQueueWriter outgoingRequestQueueWriter,
            IMapper mapper,
            ILogger<RequestDispatcher> logger)
        {
            _telegramBotClient = telegramBotClient;
            _outgoingRequestQueueWriter = outgoingRequestQueueWriter;
            _outgoingRequestQueueReader = outgoingRequestQueueReader;
            _mapper = mapper;
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

        private Task MakeRequestAsync(Request request, CancellationToken cancellationToken)
        {
            Task task = request switch
            {
                AnswerCallbackQueryRequest answerCallbackQueryRequest => _telegramBotClient.MakeRequestAsync(_mapper.Map<Telegram.Bot.Requests.AnswerCallbackQueryRequest>(answerCallbackQueryRequest), cancellationToken),
                DeleteMessageRequest deleteMessageRequest => _telegramBotClient.MakeRequestAsync(_mapper.Map<Telegram.Bot.Requests.DeleteMessageRequest>(deleteMessageRequest), cancellationToken),
                EditMessageCaptionRequest editMessageCaptionRequest => _telegramBotClient.MakeRequestAsync(_mapper.Map<Telegram.Bot.Requests.EditMessageCaptionRequest>(editMessageCaptionRequest), cancellationToken),
                EditMessageMediaRequest editMessageMediaRequest => _telegramBotClient.MakeRequestAsync(_mapper.Map<Telegram.Bot.Requests.EditMessageMediaRequest>(editMessageMediaRequest), cancellationToken),
                EditMessageReplyMarkupRequest editMessageReplyMarkupRequest => _telegramBotClient.MakeRequestAsync(_mapper.Map<Telegram.Bot.Requests.EditMessageReplyMarkupRequest>(editMessageReplyMarkupRequest), cancellationToken),
                SendMessageRequest sendMessageRequest => _telegramBotClient.MakeRequestAsync(_mapper.Map<Telegram.Bot.Requests.SendMessageRequest>(sendMessageRequest), cancellationToken),
                SendPhotoRequest sendPhotoRequest => _telegramBotClient.MakeRequestAsync(_mapper.Map<Telegram.Bot.Requests.SendPhotoRequest>(sendPhotoRequest), cancellationToken),
                _ => throw new NotImplementedException()
            };

            return task;
        }
    }
}
