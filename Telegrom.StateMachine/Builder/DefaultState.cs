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

            StateNode = new StateNode(stateType, $"Telegrom.StateNode.{stateName ?? stateType.Name}");
        }

        public DefaultState(StateNode stateNode)
        {
            StateNode = stateNode;
        }
    }
}
