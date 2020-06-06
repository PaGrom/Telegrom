using System;
using System.Collections.Generic;
using System.Linq;

namespace Telegrom.StateMachine.Builder
{
    public sealed class StateNode
    {
        internal Type StateType { get; }

        internal string GeneratedTypeName { get; set; }

        internal NextStateKind? NextStateKind { get; private set; }

        internal List<IfState> IfStates { get; private set; } = new List<IfState>();

        internal ElseState ElseState { get; private set; }

        internal StateNode(Type stateType)
        {
            StateType = stateType;
        }

        private void SetNextInternal(NextStateKind nextStateKind, ElseState elseState, params IfState[] ifStates)
        {
            NextStateKind = nextStateKind;
            ElseState = elseState;
            IfStates = ifStates.ToList();
        }

        public StateNode SetNext(NextStateKind nextStateKind, ElseState elseState)
        {
            SetNextInternal(nextStateKind, elseState);
            return ElseState.StateNode;
        }

        public (StateNode IfState, StateNode ElseState) SetNext(NextStateKind nextStateKind, IfState ifState, ElseState elseState)
        {
            SetNextInternal(nextStateKind, elseState, ifState);
            return (ifState.StateNode, elseState.StateNode);
        }

        public (StateNode IfState, StateNode IfState1, StateNode ElseState) SetNext(NextStateKind nextStateKind, IfState ifState, IfState ifState1, ElseState elseState)
        {
            SetNextInternal(nextStateKind, elseState, ifState, ifState1);
            return (ifState.StateNode, ifState1.StateNode, elseState.StateNode);
        }

        public (StateNode IfState, StateNode IfState1, StateNode IfState2, StateNode ElseState) SetNext(NextStateKind nextStateKind, IfState ifState, IfState ifState1, IfState ifState2, ElseState elseState)
        {
            SetNextInternal(nextStateKind, elseState, ifState, ifState1, ifState2);
            return (ifState.StateNode, ifState1.StateNode, ifState2.StateNode, elseState.StateNode);
        }

        public (StateNode IfState, StateNode IfState1, StateNode IfState2, StateNode IfState3, StateNode ElseState) SetNext(NextStateKind nextStateKind, IfState ifState, IfState ifState1, IfState ifState2, IfState ifState3, ElseState elseState)
        {
            SetNextInternal(nextStateKind, elseState, ifState, ifState1, ifState2, ifState3);
            return (ifState.StateNode, ifState1.StateNode, ifState2.StateNode, ifState3.StateNode, elseState.StateNode);
        }

        public (StateNode IfState, StateNode IfState1, StateNode IfState2, StateNode IfState3, StateNode IfState4, StateNode ElseState) SetNext(NextStateKind nextStateKind, IfState ifState, IfState ifState1, IfState ifState2, IfState ifState3, IfState ifState4, ElseState elseState)
        {
            SetNextInternal(nextStateKind, elseState, ifState, ifState1, ifState2, ifState3, ifState4);
            return (ifState.StateNode, ifState1.StateNode, ifState2.StateNode, ifState3.StateNode, ifState4.StateNode, elseState.StateNode);
        }
    }
}
