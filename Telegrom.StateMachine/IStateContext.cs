using System;
using System.Collections.Generic;
using Telegrom.Core.Contexts;

namespace Telegrom.StateMachine
{
    public interface IStateContext
    {
        IUpdateContext UpdateContext { get; }
        IStateMachineContext StateMachineContext { get; }
        Dictionary<string, (Type type, object value)> Attributes { get; }
    }
}
