using System;
using Telegrom.Core;

namespace Telegrom.StateMachine
{
    internal sealed class StateAttribute : ISessionAttribute
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Object { get; set; }
    }
}
