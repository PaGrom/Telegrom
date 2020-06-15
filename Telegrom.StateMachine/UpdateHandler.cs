using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Telegrom.Core;
using Telegrom.Core.Contexts;
using Telegrom.StateMachine.StateAttributes;

namespace Telegrom.StateMachine
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IUpdateContext _updateContext;
        private readonly IStateMachineConfigurationProvider _configurationProvider;
        private readonly ISessionStateAttributesService _sessionStateAttributesService;
        private readonly IStateMachineContext _stateMachineContext;
        private readonly ILogger<UpdateHandler> _logger;

        public UpdateHandler(
            ILifetimeScope lifetimeScope,
            IUpdateContext updateContext,
            IStateMachineConfigurationProvider configurationProvider,
            ISessionStateAttributesService sessionStateAttributesService,
            IStateMachineContext stateMachineContext,
            ILogger<UpdateHandler> logger)
        {
            _lifetimeScope = lifetimeScope;
            _updateContext = updateContext;
            _configurationProvider = configurationProvider;
            _sessionStateAttributesService = sessionStateAttributesService;
            _stateMachineContext = stateMachineContext;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var stateName = await _sessionStateAttributesService.GetOrSetDefaultCurrentStateAsync(_configurationProvider.InitialStateName, cancellationToken);

            _logger.LogInformation($"Current state {stateName}");

            IState state;

            try
            {
                state = _lifetimeScope.ResolveNamed<IState>(stateName);
                await state.OnEnter(cancellationToken);
                await state.Handle(cancellationToken);
            }
            catch
            {
                _logger.LogError($"State {stateName} doesn't exists or failed. Set default state: {_configurationProvider.DefaultStateName}");
                stateName = _configurationProvider.DefaultStateName;
                state = _lifetimeScope.ResolveNamed<IState>(stateName);
                await state.OnEnter(cancellationToken);
                await state.Handle(cancellationToken);
            }

            if (_stateMachineContext.NextStateName == null)
            {
                return;
            }

            while (_stateMachineContext.NextStateName != null)
            {
                await state.OnExit(cancellationToken);
                stateName = _stateMachineContext.NextStateName;
                _logger.LogInformation($"Next state is {stateName}");
                await _sessionStateAttributesService.UpdateCurrentStateAsync(stateName, cancellationToken);
                state = _lifetimeScope.ResolveNamed<IState>(stateName);
                _stateMachineContext.Reset();
                await state.OnEnter(cancellationToken);
            }
        }
    }

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
