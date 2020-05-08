using System.Threading;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.StateMachine
{
    public interface IState
    {
        Task OnEnter(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task Handle(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task OnExit(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
