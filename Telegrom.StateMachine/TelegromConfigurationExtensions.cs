using System;
using Telegrom.Core.Configuration;

namespace Telegrom.StateMachine
{
    public static class TelegromConfigurationExtensions
    {
        public static ITelegromConfiguration AddStateMachineBuilder(
            this ITelegromConfiguration configuration,
            StateMachineBuilder stateMachineBuilder)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (stateMachineBuilder == null) throw new ArgumentNullException(nameof(stateMachineBuilder));

            return configuration.Use(stateMachineBuilder, x => StateMachineBuilder.Current = x);
        }
    }
}
