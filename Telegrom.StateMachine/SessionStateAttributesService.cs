using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core;
using Telegrom.StateMachine.StateAttributes;

namespace Telegrom.StateMachine
{
    internal sealed class SessionStateAttributesService : ISessionStateAttributesService
    {
        private readonly ISessionAttributesService _sessionAttributesService;

        public SessionStateAttributesService(ISessionAttributesService sessionAttributesService)
        {
            _sessionAttributesService = sessionAttributesService;
        }

        public async Task<string> GetOrSetDefaultCurrentStateAsync(string defaultStateName, CancellationToken cancellationToken)
        {
            var currentSessionState =
                (await _sessionAttributesService.GetAllByTypeAsync<CurrentSessionState>(cancellationToken))
                .SingleOrDefault();

            if (currentSessionState != null)
            {
                return currentSessionState.StateName;
            }

            currentSessionState = new CurrentSessionState
            {
                Id = Guid.NewGuid(),
                StateName = defaultStateName
            };

            await _sessionAttributesService.SaveOrUpdateSessionAttributeAsync(currentSessionState.Id, currentSessionState, cancellationToken);

            return currentSessionState.StateName;
        }

        public async Task UpdateCurrentStateAsync(string stateName, CancellationToken cancellationToken)
        {
            var currentSessionState =
                (await _sessionAttributesService.GetAllByTypeAsync<CurrentSessionState>(cancellationToken))
                .SingleOrDefault() ?? new CurrentSessionState
                {
                    Id = Guid.NewGuid()
                };

            currentSessionState.StateName = stateName;

            await _sessionAttributesService.SaveOrUpdateSessionAttributeAsync(currentSessionState.Id, currentSessionState, cancellationToken);
        }
    }
}
