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
        private readonly IIdentityService _identityService;
        private readonly IStateMachineContext _stateMachineContext;
        private readonly ILogger<UpdateHandler> _logger;

        public UpdateHandler(
            ILifetimeScope lifetimeScope,
            IUpdateContext updateContext,
            IStateMachineConfigurationProvider configurationProvider,
            IIdentityService identityService,
            IStateMachineContext stateMachineContext,
            ILogger<UpdateHandler> logger)
        {
            _lifetimeScope = lifetimeScope;
            _updateContext = updateContext;
            _configurationProvider = configurationProvider;
            _identityService = identityService;
            _stateMachineContext = stateMachineContext;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var stateName = await _identityService.GetOrSetDefaultCurrentStateAsync(_updateContext.SessionContext.User,
                _configurationProvider.InitialStateName, cancellationToken);

            _logger.LogInformation($"Current state {stateName}");

            IState state;

            try
            {
                state = _lifetimeScope.ResolveNamed<IState>(stateName);
            }
            catch
            {
                _logger.LogError($"State {stateName} doesn't exists. Set default state: {_configurationProvider.DefaultStateName}");
                stateName = _configurationProvider.DefaultStateName;
                state = _lifetimeScope.ResolveNamed<IState>(stateName);
            }

            await state.OnEnter(cancellationToken);
            await state.Handle(cancellationToken);

            if (_stateMachineContext.NextStateName == null)
            {
                return;
            }

            while (_stateMachineContext.NextStateName != null)
            {
                await state.OnExit(cancellationToken);
                stateName = _stateMachineContext.NextStateName;
                _logger.LogInformation($"Next state is {stateName}");
                await _identityService.UpdateCurrentStateAsync(_updateContext.SessionContext.User, stateName, cancellationToken);
                state = _lifetimeScope.ResolveNamed<IState>(stateName);
                _stateMachineContext.Reset();
                await state.OnEnter(cancellationToken);
            }
        }
    }
}
