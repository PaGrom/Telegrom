using System;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;

namespace Telegrom.Core.TelegramModel
{
    public abstract class Request
    {
        internal abstract object Instance { get; }

        internal abstract Type GenericArgumentType { get; }

        internal abstract TaskCompletionSource<object> TaskCompletionSource { get; }

        public static Request Wrap<TResponse>(IRequest<TResponse> request)
        {
            return new RequestImpl<TResponse>(request, new TaskCompletionSource<object>());
        }

        public static Request Wrap<TResponse>(IRequest<TResponse> request, TaskCompletionSource<object> taskCompletionSource)
        {
            return new RequestImpl<TResponse>(request, taskCompletionSource);
        }

        private class RequestImpl<TResponse> : Request
        {
            public RequestImpl(IRequest<TResponse> request, TaskCompletionSource<object> taskCompletionSource)
            {
                Instance = request;
                TaskCompletionSource = taskCompletionSource;
            }

            internal override object Instance { get; }
            internal override Type GenericArgumentType => typeof(TResponse);
            internal override TaskCompletionSource<object> TaskCompletionSource { get; }
        }
    }
}
