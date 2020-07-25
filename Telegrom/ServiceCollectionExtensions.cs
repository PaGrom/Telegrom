using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Telegrom
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelegromBot(this IServiceCollection services, Action<ContainerBuilder> configurationAction = null)
        {
            return services.AddHostedService<TelegromBot>();
        }
    }
}
