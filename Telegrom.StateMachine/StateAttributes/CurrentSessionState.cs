using System;
using Telegrom.Core;

namespace Telegrom.StateMachine.StateAttributes
{
    internal sealed class CurrentSessionState : ISessionAttribute
    {
        public Guid Id { get; set; }
        public string StateName { get; set; }
    }
}
