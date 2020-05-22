using System.Threading;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.StateMachine
{
    public interface IState
    {
        Task OnEnter(CancellationToken cancellationToken);
        Task Handle(CancellationToken cancellationToken);
        Task OnExit(CancellationToken cancellationToken);
    }
}
