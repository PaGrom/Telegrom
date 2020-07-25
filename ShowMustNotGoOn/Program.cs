using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegrom;

namespace ShowMustNotGoOn
{
    public static class Program
    {
        private static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) => ContainerConfiguration.Init(hostContext.Configuration, builder))
                .ConfigureServices(services => services.AddTelegromBot());
    }
}
