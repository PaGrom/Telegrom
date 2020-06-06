using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.StateMachine
{
    public abstract class StateBase : IState
    {
        public virtual Task OnEnter(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task Handle(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnExit(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
