using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegrom.Core;

namespace Telegrom.Database
{
    public class WakeUpService : IWakeUpService
    {
        private readonly DbContextOptions _dbContextOptions;

        public WakeUpService(DbContextOptions dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public async Task WakeUpAsync(Func<Update, CancellationToken, Task> handler, CancellationToken cancellationToken)
        {
            await using var context = new DatabaseContext(_dbContextOptions);
            var notProcessedUpdates = await context.SessionUpdates
                .Where(u => !u.Processed)
                .ToListAsync(cancellationToken);

            foreach (var sessionUpdate in notProcessedUpdates)
            {
                var updateType = Type.GetType(sessionUpdate.UpdateType);
                var updateObject = (Update)JsonConvert.DeserializeObject(sessionUpdate.Update, updateType);
                handler.Invoke(updateObject, cancellationToken);
            }
        }
    }
}
