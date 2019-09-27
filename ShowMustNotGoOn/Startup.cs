using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShowMustNotGoOn
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            ContainerConfiguration.Init(Configuration, builder);
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}
