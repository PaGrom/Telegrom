using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Telegrom.Core;
using Telegrom.Core.Contexts;

namespace Telegrom.StateMachine
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IUpdateContext _updateContext;
        private readonly IStateMachineConfigurationProvider _configurationProvider;
        private readonly IStateMachineContext _stateMachineContext;
        private readonly IIdentityStatesService _identityStatesService;
        private readonly IEnumerable<IUpdateInterceptor> _updateInterceptors;
        private readonly ILogger<UpdateHandler> _logger;

        public UpdateHandler(
            ILifetimeScope lifetimeScope,
            IUpdateContext updateContext,
            IStateMachineConfigurationProvider configurationProvider,
            IStateMachineContext stateMachineContext,
            IIdentityStatesService identityStatesService,
            IEnumerable<IUpdateInterceptor> updateInterceptors,
            ILogger<UpdateHandler> logger)
        {
            _lifetimeScope = lifetimeScope;
            _updateContext = updateContext;
            _configurationProvider = configurationProvider;
            _stateMachineContext = stateMachineContext;
            _identityStatesService = identityStatesService;
            _updateInterceptors = updateInterceptors;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var stateName = await _identityStatesService.GetOrSetDefaultCurrentStateAsync(_configurationProvider.InitialStateName, cancellationToken);

            try
            {
                foreach (var updateInterceptor in _updateInterceptors)
                {
                    if (!updateInterceptor.NonInterceptableStates.Contains(stateName))
                    {
                        await updateInterceptor.BeforeHandle(cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Get exception during update interceptor handle for user {_updateContext.SessionContext.User.Id}");
                return;
            }

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
                await _identityStatesService.UpdateCurrentStateAsync(stateName, cancellationToken);
                state = _lifetimeScope.ResolveNamed<IState>(stateName);
                _stateMachineContext.Reset();
                await state.OnEnter(cancellationToken);
            }
        }
    }
}
