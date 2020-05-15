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

        Task<bool> Handle(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        Task OnExit(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
