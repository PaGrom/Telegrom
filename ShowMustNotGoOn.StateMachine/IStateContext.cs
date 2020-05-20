using System;
using System.Collections.Generic;
using ShowMustNotGoOn.Core.Contexts;

namespace ShowMustNotGoOn.StateMachine
{
    public interface IStateContext
    {
        IUpdateContext UpdateContext { get; }
        IStateMachineContext StateMachineContext { get; }
        Dictionary<string, (Type type, object value)> Attributes { get; }
    }
}
