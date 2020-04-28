using Autofac.Builder;
using ShowMustNotGoOn.Core.Request;
using ShowMustNotGoOn.Core.Session;

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

        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerRequest<TLimit,
            TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.InstancePerMatchingLifetimeScope(typeof(RequestContext));
        }
    }
}
