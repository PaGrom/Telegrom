using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegrom.Core;

namespace Telegrom.Database
{
    public sealed class SessionStateAttributesRemover : ISessionStateAttributesRemover
    {
        private readonly DbContextOptions _dbContextOptions;

        public SessionStateAttributesRemover(DbContextOptions dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public async Task RemoveAllSessionStateAttributesAsync(User user, CancellationToken cancellationToken)
        {
            await using var context = new DatabaseContext(_dbContextOptions);

            var sessionStateAttributes = await context.SessionAttributes
                .Where(a => a.SessionId == user.Id)
                .ToListAsync(cancellationToken);

            context.SessionAttributes.RemoveRange(sessionStateAttributes);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
