﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram.Bot.Requests.Abstractions;
using Telegrom.Core;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;
using Telegrom.Database;
using Telegrom.Database.Model;

namespace Telegrom
{
    public sealed class TelegromClient
    {
        private readonly IGlobalIncomingUpdateQueueWriter _incomingUpdateQueueWriter;
        private readonly IGlobalOutgoingRequestQueueWriter _outgoingRequestQueueWriter;
        private readonly DatabaseContext _databaseContext;

        public IGlobalAttributesService GlobalAttributesService { get; }

        public TelegromClient(DbContextOptions dbContextOptions,
            IGlobalIncomingUpdateQueueWriter incomingUpdateQueueWriter,
            IGlobalOutgoingRequestQueueWriter outgoingRequestQueueWriter,
            IGlobalAttributesService globalAttributesService)
        {
            _incomingUpdateQueueWriter = incomingUpdateQueueWriter;
            _outgoingRequestQueueWriter = outgoingRequestQueueWriter;
            GlobalAttributesService = globalAttributesService;
            _databaseContext = new DatabaseContext(dbContextOptions);
        }

        public IAsyncEnumerable<IdentityUser> GetUsers()
        {
            return _databaseContext.IdentityUsers.AsAsyncEnumerable();
        }

        public IAsyncEnumerable<T> GetUserAttributes<T>(IdentityUser user) where T : ISessionAttribute
        {
            return _databaseContext.SessionAttributes
                .Where(a => a.SessionId == user.Id && a.Type == typeof(T).FullName)
                .Select(a => JsonConvert.DeserializeObject<T>(a.Value))
                .AsAsyncEnumerable();
        }

        public async Task<TResponse> PostRequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            var req = Request.Wrap(request, taskCompletionSource);
            await _outgoingRequestQueueWriter.EnqueueAsync(req, cancellationToken);
            return (TResponse)await taskCompletionSource.Task;
        }
    }
}
