using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.StateMachine
{
    public interface IUpdateInterceptor
    {
        Task<bool> BeforeHandle(CancellationToken cancellationToken);
        IEnumerable<string> NonInterceptableStates { get; }
    }
}
