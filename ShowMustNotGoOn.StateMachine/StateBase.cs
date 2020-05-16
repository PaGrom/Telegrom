using System.Threading;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.StateMachine
{
    public abstract class StateBase : IState
    {
        public virtual Task<bool> OnEnter(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> Handle(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> OnExit(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
