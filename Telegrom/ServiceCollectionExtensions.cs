using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Telegrom
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelegromBot(this IServiceCollection services, Action<ContainerBuilder> configurationAction = null)
        {
            return services.AddSingleton<TelegromBot>()
                .AddSingleton<IHostedService>(p => p.GetService<TelegromBot>());
        }
    }
}
