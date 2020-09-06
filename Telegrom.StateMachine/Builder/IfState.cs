using System;
using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.StateMachine.Builder
{
    public sealed class IfState
    {
        internal Func<IStateContext, CancellationToken, Task<bool>> Condition { get; }
        internal StateNode StateNode { get; }

        public IfState(Func<IStateContext, CancellationToken, Task<bool>> condition, Type stateType, string stateName = null)
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

        public IfState(Func<IStateContext, bool> condition, Type stateType, string stateName = null)
            : this((context, ctk) => Task.Run(() => condition(context), ctk), stateType, stateName)
        { }

        public IfState(Func<IStateContext, CancellationToken, Task<bool>> condition, StateNode stateNode)
        {
            Condition = condition;
            StateNode = stateNode;
        }

        public IfState(Func<IStateContext, bool> condition, StateNode stateNode)
            : this((context, ctk) => Task.Run(() => condition(context), ctk), stateNode)
        { }
    }
}
