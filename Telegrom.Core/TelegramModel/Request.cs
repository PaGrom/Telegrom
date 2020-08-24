using System;
using System.Dynamic;
using System.Linq;
using Telegram.Bot.Requests.Abstractions;

namespace Telegrom.Core.TelegramModel
{
    public abstract class Request
    {
        internal abstract object Instance { get; }

        internal abstract Type GenericArgumentType { get; }

        public static Request Wrap<TResponse>(IRequest<TResponse> request)
        {
            return new RequestImpl<TResponse>(request);
        }

        private class RequestImpl<TResponse> : Request
        {
            private readonly IRequest<TResponse> _instance;

            public RequestImpl(IRequest<TResponse> request)
            {
                _instance = request;
            }

            internal override object Instance => _instance;
            internal override Type GenericArgumentType => typeof(TResponse);
        }
    }
}
