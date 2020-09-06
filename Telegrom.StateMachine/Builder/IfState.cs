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
                throw new ArgumentException($"Type must implement interface {nameof(IState)}", nameof(stateType));
            }

            if (stateName != null && string.IsNullOrWhiteSpace(stateName))
            {
                throw new ArgumentException("State name cannot be empty", nameof(stateName));
            }

            Condition = condition;
            StateNode = new StateNode(stateType, $"{StateNode.StateNamePrefix}{stateName ?? stateType.Name}");
        }

        public IfState(Func<IStateContext, Task<bool>> condition, StateNode stateNode)
        {
            Condition = condition;
            StateNode = stateNode;
        }

        public IfState(Func<IStateContext, bool> condition, StateNode stateNode)
        {
            Condition = context => new Task<bool>(() => condition(context));
            StateNode = stateNode;
        }
    }
}
