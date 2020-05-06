using Autofac.Builder;
using ShowMustNotGoOn.Core.Contexts;

namespace ShowMustNotGoOn.Core.Extensions
{
    public static class AutofacExtensions
    {
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerSession<TLimit,
            TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.InstancePerMatchingLifetimeScope(typeof(SessionContext));
        }

        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerUpdate<TLimit,
            TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.InstancePerMatchingLifetimeScope(typeof(UpdateContext));
        }
    }
}
