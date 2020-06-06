using System;

namespace Telegrom.StateMachine.Builder
{
    public sealed class ElseState
    {
        internal StateNode StateNode { get; }

        public ElseState(Type stateType)
        {
            if (!typeof(IState).IsAssignableFrom(stateType))
            {
                throw new ArgumentException(nameof(stateType), $"Type must implement interface {nameof(IState)}");
            }

            StateNode = new StateNode(stateType);
        }

        public ElseState(StateNode stateNode)
        {
            StateNode = stateNode;
        }
    }
}
