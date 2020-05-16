using System.Threading;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.StateMachine
{
    public interface IState
    {
        Task<bool> OnEnter(CancellationToken cancellationToken);
        Task<bool> Handle(CancellationToken cancellationToken);
        Task<bool> OnExit(CancellationToken cancellationToken);
    }
}
