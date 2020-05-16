using System;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.StateMachine.Builder
{
    public sealed class ConditionalNextState
    {
        public Type NextState { get; }
        public IStateNode NextStateNode { get; }
        public NextStateType NextStateType { get; }
        public Func<IStateContext, Task<bool>> Condition { get; }

        public ConditionalNextState(Type nextState, NextStateType nextStateType, Func<IStateContext, Task<bool>> condition)
        {
            if (!typeof(IState).IsAssignableFrom(nextState))
            {
                throw new ArgumentException(nameof(nextState), $"Type must implement interface {nameof(IState)}");
            }

            NextState = nextState;
            NextStateType = nextStateType;
            Condition = condition;
        }

        public ConditionalNextState(IStateNode nextStateNode, NextStateType nextStateType, Func<IStateContext, Task<bool>> condition)
        {
            NextStateNode = nextStateNode;
            NextStateType = nextStateType;
            Condition = condition;
        }
    }
}
