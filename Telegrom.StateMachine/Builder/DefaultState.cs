using System;

namespace Telegrom.StateMachine.Builder
{
    public sealed class DefaultState
    {
        internal StateNode StateNode { get; }

        public DefaultState(Type stateType, string stateName = null)
        {
            if (!typeof(IState).IsAssignableFrom(stateType))
            {
                throw new ArgumentException(nameof(stateType), $"Type must implement interface {nameof(IState)}");
            }

            if (stateName != null && string.IsNullOrWhiteSpace(stateName))
            {
                throw new ArgumentException("State name cannot be empty", nameof(stateName));
            }

            StateNode = new StateNode(stateType, $"{StateNode.StateNamePrefix}{stateName ?? stateType.Name}");
        }

        public DefaultState(StateNode stateNode)
        {
            StateNode = stateNode;
        }
    }
}
