﻿using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.MessageBus
{
    public interface IIncomingUpdateQueueReader
    {
        ValueTask<Update> DequeueAsync(CancellationToken cancellationToken);
    }
}
