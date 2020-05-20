using System;
using System.Collections.Generic;
using ShowMustNotGoOn.Core.Contexts;

namespace ShowMustNotGoOn.StateMachine
{
    public sealed class StateContext : IStateContext
    {
        public IUpdateContext UpdateContext { get; }
        public IStateMachineContext StateMachineContext { get; }
        public Dictionary<string, (Type type, object value)> Attributes { get; } = new Dictionary<string, (Type type, object value)>();

        public StateContext(IUpdateContext updateContext,
            IStateMachineContext stateMachineContext)
        {
            UpdateContext = updateContext;
            StateMachineContext = stateMachineContext;
        }
    }
}
