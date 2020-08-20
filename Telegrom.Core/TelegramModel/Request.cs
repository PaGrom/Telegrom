using System;
using System.Linq;
using Telegram.Bot.Requests.Abstractions;

namespace Telegrom.Core.TelegramModel
{
    public sealed class Request
    {
        internal object RequestObject { get; }

        internal Type RequestType { get; }

        internal Type IRequestGenericArgumentType
        {
            get
            {
                var iRequestInterfaceType = RequestType.GetInterfaces()
                    .Single(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IRequest<>));

                return iRequestInterfaceType.GetGenericArguments()[0];
            }
        }

        public Request(object telegramRequest)
        {
            if (!telegramRequest.GetType().GetInterfaces()
                .Any(x => 
                    x.IsGenericType && 
                    x.GetGenericTypeDefinition() == typeof(IRequest<>)))
            {
                throw new ArgumentException($"Request has to implement {typeof(IRequest<>).FullName} interface");
            }

            RequestObject = telegramRequest;
            RequestType = telegramRequest.GetType();
        }
    }
}
