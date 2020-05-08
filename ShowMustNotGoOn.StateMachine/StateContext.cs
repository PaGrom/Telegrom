﻿using ShowMustNotGoOn.Core.Contexts;

namespace ShowMustNotGoOn.StateMachine
{
    public sealed class StateContext : IStateContext
    {
        public IUpdateContext UpdateContext { get; }
        public IStateMachineContext StateMachineContext { get; }

        public StateContext(IUpdateContext updateContext, IStateMachineContext stateMachineContext)
        {
            UpdateContext = updateContext;
            StateMachineContext = stateMachineContext;
        }
    }
}
