using System.Threading;
using System.Threading.Tasks;
using Autofac;
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

        public UpdateHandler(
            ILifetimeScope lifetimeScope,
            IUpdateContext updateContext,
            IStateMachineConfigurationProvider configurationProvider,
            IUsersService usersService,
            IStateMachineContext stateMachineContext)
        {
            _lifetimeScope = lifetimeScope;
            _updateContext = updateContext;
            _configurationProvider = configurationProvider;
            _usersService = usersService;
            _stateMachineContext = stateMachineContext;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var stateName = await _usersService.GetOrSetDefaultCurrentStateAsync(_updateContext.SessionContext.User,
                _configurationProvider.InitialState.Name, cancellationToken);

            var state = _lifetimeScope.ResolveNamed<IState>(stateName);
            await state.Handle(cancellationToken);

            if (_stateMachineContext.NextState == null)
            {
                return;
            }

            while (_stateMachineContext.NextState != null)
            {
                await state.OnExit(cancellationToken);
                stateName = _stateMachineContext.NextState.Name;
                await _usersService.UpdateCurrentStateAsync(_updateContext.SessionContext.User, stateName, cancellationToken);
                state = _lifetimeScope.ResolveNamed<IState>(stateName);
                _stateMachineContext.Reset();
                await state.OnEnter(cancellationToken);
            }
        }
    }
}
