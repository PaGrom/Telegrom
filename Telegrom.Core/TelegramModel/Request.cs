using System;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;

namespace Telegrom.Core.TelegramModel
{
    public abstract class Request
    {
        internal abstract object Instance { get; }

        internal abstract Type GenericArgumentType { get; }

        internal abstract Func<object, Task> Callback { get; }

        public static Request Wrap<TResponse>(IRequest<TResponse> request, Func<object, Task> callback)
        {
            return new RequestImpl<TResponse>(request, callback);
        }

        private class RequestImpl<TResponse> : Request
        {
            private readonly IRequest<TResponse> _instance;

            public RequestImpl(IRequest<TResponse> request, Func<object, Task> callback)
            {
                _instance = request;
                Callback = callback;
            }

            internal override object Instance => _instance;
            internal override Type GenericArgumentType => typeof(TResponse);
            internal override Func<object, Task> Callback { get; }
        }
    }
}
