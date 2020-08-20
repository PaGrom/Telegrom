using System;
using Telegram.Bot.Requests.Abstractions;

namespace Telegrom.Core.TelegramModel
{
    public sealed class Request
    {
        internal object RequestObject { get; }

        internal Type RequestType { get; }

        public Request(object telegramRequest)
        {
            if (!telegramRequest.GetType().IsAssignableFrom(typeof(IRequest<>)))
            {
                throw new ArgumentException($"Request has to implement {typeof(IRequest<>).FullName} interface");
            }

            RequestObject = telegramRequest;
            RequestType = telegramRequest.GetType();
        }
    }
}
