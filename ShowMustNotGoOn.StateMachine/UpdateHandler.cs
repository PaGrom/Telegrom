using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Contexts;

namespace ShowMustNotGoOn.StateMachine
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IUpdateContext _updateContext;
        private readonly IStateMachineConfigurationProvider _configurationProvider;
        private readonly IUsersService _usersService;
        private readonly IStateMachineContext _stateMachineContext;
        private readonly ILogger<UpdateHandler> _logger;

        public UpdateHandler(
            ILifetimeScope lifetimeScope,
            IUpdateContext updateContext,
            IStateMachineConfigurationProvider configurationProvider,
            IUsersService usersService,
            IStateMachineContext stateMachineContext,
            ILogger<UpdateHandler> logger)
        {
            _lifetimeScope = lifetimeScope;
            _updateContext = updateContext;
            _configurationProvider = configurationProvider;
            _usersService = usersService;
            _stateMachineContext = stateMachineContext;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var stateName = await _usersService.GetOrSetDefaultCurrentStateAsync(_updateContext.SessionContext.User,
                _configurationProvider.InitialStateName, cancellationToken);

            _logger.LogInformation($"Current state {stateName}");

            var state = _lifetimeScope.ResolveNamed<IState>(stateName);
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
                await _usersService.UpdateCurrentStateAsync(_updateContext.SessionContext.User, stateName, cancellationToken);
                state = _lifetimeScope.ResolveNamed<IState>(stateName);
                _stateMachineContext.Reset();
                await state.OnEnter(cancellationToken);
            }
        }
    }
}
