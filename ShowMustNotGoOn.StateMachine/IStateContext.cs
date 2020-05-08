using ShowMustNotGoOn.Core.Contexts;

namespace ShowMustNotGoOn.StateMachine
{
    public interface IStateContext
    {
        IUpdateContext UpdateContext { get; }
        IStateMachineContext StateMachineContext { get; }
    }
}
