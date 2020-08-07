using System;
using System.Collections.Generic;
using System.Linq;

namespace Telegrom.StateMachine.Builder
{
    public sealed class StateNode
    {
        internal Type StateType { get; }

        internal string StateName { get; }

        internal NextStateKind? NextStateKind { get; private set; }

        internal bool Built { get; set; }

        internal List<IfState> IfStates { get; private set; } = new List<IfState>();

        internal DefaultState DefaultState { get; private set; }

        internal StateNode(Type stateType, string stateName)
        {
            StateType = stateType;
            StateName = stateName;
        }

        private void SetNextInternal(NextStateKind nextStateKind, DefaultState defaultState, params IfState[] ifStates)
        {
            NextStateKind = nextStateKind;
            DefaultState = defaultState;
            IfStates = ifStates.ToList();
        }

        public StateNode SetNext(NextStateKind nextStateKind, DefaultState defaultState)
        {
            SetNextInternal(nextStateKind, defaultState);
            return DefaultState.StateNode;
        }

        public (StateNode IfState, StateNode DefaultState) SetNext(NextStateKind nextStateKind, IfState ifState, DefaultState defaultState)
        {
            SetNextInternal(nextStateKind, defaultState, ifState);
            return (ifState.StateNode, defaultState.StateNode);
        }

        public (StateNode IfState, StateNode IfState1, StateNode DefaultState) SetNext(NextStateKind nextStateKind, IfState ifState, IfState ifState1, DefaultState defaultState)
        {
            SetNextInternal(nextStateKind, defaultState, ifState, ifState1);
            return (ifState.StateNode, ifState1.StateNode, defaultState.StateNode);
        }

        public (StateNode IfState, StateNode IfState1, StateNode IfState2, StateNode DefaultState) SetNext(NextStateKind nextStateKind, IfState ifState, IfState ifState1, IfState ifState2, DefaultState defaultState)
        {
            SetNextInternal(nextStateKind, defaultState, ifState, ifState1, ifState2);
            return (ifState.StateNode, ifState1.StateNode, ifState2.StateNode, defaultState.StateNode);
        }

        public (StateNode IfState, StateNode IfState1, StateNode IfState2, StateNode IfState3, StateNode DefaultState) SetNext(NextStateKind nextStateKind, IfState ifState, IfState ifState1, IfState ifState2, IfState ifState3, DefaultState defaultState)
        {
            SetNextInternal(nextStateKind, defaultState, ifState, ifState1, ifState2, ifState3);
            return (ifState.StateNode, ifState1.StateNode, ifState2.StateNode, ifState3.StateNode, defaultState.StateNode);
        }

        public (StateNode IfState, StateNode IfState1, StateNode IfState2, StateNode IfState3, StateNode IfState4, StateNode DefaultState) SetNext(NextStateKind nextStateKind, IfState ifState, IfState ifState1, IfState ifState2, IfState ifState3, IfState ifState4, DefaultState defaultState)
        {
            SetNextInternal(nextStateKind, defaultState, ifState, ifState1, ifState2, ifState3, ifState4);
            return (ifState.StateNode, ifState1.StateNode, ifState2.StateNode, ifState3.StateNode, ifState4.StateNode, defaultState.StateNode);
        }
    }
}
