﻿using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.MessageBus
{
    public interface IOutgoingRequestQueueWriter
    {
        ValueTask EnqueueAsync(Request request, CancellationToken cancellationToken);
    }
}
