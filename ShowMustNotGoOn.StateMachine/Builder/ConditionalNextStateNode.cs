using System;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.StateMachine.Builder
{
    internal sealed class ConditionalNextStateNode
    {
        public StateNode NextStateNode { get; set; }

        public NextStateType NextStateType { get; set; }

        public Func<IStateContext, Task<bool>> Condition { get; set; }
    }
}
