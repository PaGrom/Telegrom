using System;
using System.Linq;
using Telegram.Bot.Requests.Abstractions;

namespace Telegrom.Core.TelegramModel
{
    public abstract class RequestBase
    {
        internal abstract object Instance { get; }

        internal abstract Type GenericArgumentType { get; }
    }

    public class Request<TResponse> : RequestBase
    {
        private readonly IRequest<TResponse> _instance;

        public Request(IRequest<TResponse> request)
        {
            _instance = request;
        }

        internal override object Instance => _instance;
        internal override Type GenericArgumentType => typeof(TResponse);
    }
}
