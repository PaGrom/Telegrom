using System;
using System.Threading.Tasks;

namespace Telegrom.StateMachine.Builder
{
    public sealed class IfState
    {
        internal Func<IStateContext, Task<bool>> Condition { get; }
        internal StateNode StateNode { get; }

        public IfState(Func<IStateContext, Task<bool>> condition, Type stateType, string stateName = null)
        {
            if (!typeof(IState).IsAssignableFrom(stateType))
            {
                throw new ArgumentException(nameof(stateType), $"Type must implement interface {nameof(IState)}");
            }

            Condition = condition;
            StateNode = new StateNode(stateType, stateName ?? stateType.Name);
        }

        public IfState(Func<IStateContext, Task<bool>> condition, StateNode stateNode)
        {
            Condition = condition;
            StateNode = stateNode;
        }
    }
}
