using System;

namespace Telegrom.StateMachine.StateAttributes
{
    internal sealed class CurrentSessionState
    {
        public Guid Id { get; set; }
        public string StateName { get; set; }
    }
}
