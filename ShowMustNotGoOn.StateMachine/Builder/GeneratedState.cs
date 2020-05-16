using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.StateMachine.Builder
{
    internal class GeneratedState : IState
    {
        private readonly IState _current;
        private readonly StateNode _stateNode;
        private readonly IStateContext _stateContext;

        public GeneratedState(IState current, StateNode stateNode, IStateContext stateContext)
        {
            _current = current;
            _stateNode = stateNode;
            _stateContext = stateContext;
        }

        public async Task<bool> OnEnter(CancellationToken cancellationToken)
        {
            if (!await _current.OnEnter(cancellationToken))
            {
                return false;
            }

            await MoveNextAsync(NextStateType.AfterOnEnter);
            return true;
        }

        public async Task<bool> Handle(CancellationToken cancellationToken)
        {
            if (!await _current.Handle(cancellationToken))
            {
                return false;
            }

            await MoveNextAsync(NextStateType.AfterHandle);
            return true;
        }

        public async Task<bool> OnExit(CancellationToken cancellationToken)
        {
            if (!await _current.OnExit(cancellationToken))
            {
                return false;
            }

            await MoveNextAsync(NextStateType.AfterOnExit);
            return true;
        }

        private async Task MoveNextAsync(NextStateType nextStateType)
        {
            var conditionalNextStateNodes = _stateNode.ConditionalNextStateNodes
                .Where(n => n.NextStateType == nextStateType)
                .ToList();

            if (!conditionalNextStateNodes.Any())
            {
                return;
            }

            var nextConditionalStateFound = false;

            foreach (var conditionalNextStateNode in conditionalNextStateNodes)
            {
                if (await conditionalNextStateNode.Condition(_stateContext))
                {
                    _stateContext.StateMachineContext.MoveTo(conditionalNextStateNode.NextStateNode.GeneratedTypeName);
                    nextConditionalStateFound = true;
                    break;
                }
            }

            if (!nextConditionalStateFound)
            {
                _stateContext.StateMachineContext.Reset();
            }
        }
    }
}
