using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core;
using Telegrom.Core.Contexts;
using Telegrom.Database.Model;

namespace Telegrom.Database
{
    public class IdentityStatesService : IIdentityStatesService
    {
        private readonly DatabaseContext _context;
        private readonly SessionContext _sessionContext;

        public IdentityStatesService(DatabaseContext context, SessionContext sessionContext)
        {
            _context = context;
            _sessionContext = sessionContext;
        }

        public async Task<string> GetOrSetDefaultCurrentStateAsync(string defaultStateName, CancellationToken cancellationToken)
        {
            var identityState = await _context.IdentityStates
                .FindAsync(new object[] {_sessionContext.User.Id}, cancellationToken);

            if (identityState != null)
            {
                return identityState.StateName;
            }

            identityState = new IdentityState
            {
                IdentityId = _sessionContext.User.Id,
                StateName = defaultStateName
            };

            await _context.IdentityStates.AddAsync(identityState, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return identityState.StateName;
        }

        public async Task UpdateCurrentStateAsync(string stateName, CancellationToken cancellationToken)
        {
            var identityState = await _context.IdentityStates
                .FindAsync(new object[] {_sessionContext.User.Id}, cancellationToken);

            if (identityState == null)
            {
                identityState = new IdentityState
                {
                    IdentityId = _sessionContext.User.Id,
                    StateName = stateName
                };

                await _context.IdentityStates.AddAsync(identityState, cancellationToken);
            }
            else
            {
                identityState.StateName = stateName;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
