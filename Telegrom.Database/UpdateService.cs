using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegrom.Core;
using Telegrom.Database.Model;

namespace Telegrom.Database
{
    public class UpdateService : IUpdateService
    {
        private readonly User _user;
        private readonly DatabaseContext _context;

        public UpdateService(User user, DatabaseContext context)
        {
            _user = user;
            _context = context;
        }

        public async Task SaveUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            var sessionUpdate = await _context.SessionUpdates
                .FindAsync(new object[] { _user.Id, update.Id }, cancellationToken);

            if (sessionUpdate != null)
            {
                sessionUpdate.Processed = false;
            }
            else
            {
                await _context.SessionUpdates.AddAsync(new SessionUpdate
                {
                    IdentityId = _user.Id,
                    UpdateId = update.Id,
                    UpdateType = update.GetType().AssemblyQualifiedName,
                    Update = JsonConvert.SerializeObject(update)
                }, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task MakeUpdateProcessedAsync(Update update, CancellationToken cancellationToken)
        {
            var sessionUpdate = await _context.SessionUpdates
                .FindAsync(new object[] { _user.Id, update.Id }, cancellationToken);

            sessionUpdate.Processed = true;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
