using System;

namespace ShowMustNotGoOn.StateMachine
{
    public interface IStateMachineContext
    {
        Type NextState { get; }
        void MoveTo<T>() where T : IState;
        void MoveTo(Type type);
        void Reset();
    }
}
