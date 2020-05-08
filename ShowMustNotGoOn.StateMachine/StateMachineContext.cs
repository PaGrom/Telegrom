using System;

namespace ShowMustNotGoOn.StateMachine
{
    internal class StateMachineContext : IStateMachineContext
    {
        public StateMachineContext()
        {
            NextState = null;
        }

        public Type NextState { get; private set; }

        public void MoveTo<T>() where T : IState
        {
            NextState = typeof(T);
        }

        public void MoveTo(Type type)
        {
            NextState = type;
        }

        public void Reset()
        {
            NextState = null;
        }
    }
}
