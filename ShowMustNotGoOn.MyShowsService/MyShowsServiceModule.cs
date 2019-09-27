using Autofac;
using AutoMapper;
using RestSharp;
using ShowMustNotGoOn.Core;

namespace ShowMustNotGoOn.MyShowsService
{
    public class MyShowsServiceModule : Module
    {
        public string MyShowsApiUrl { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new RestClient(MyShowsApiUrl))
                .As<IRestClient>();

            builder.RegisterType<MyShowsService>()
                .As<IMyShowsService>();

            builder.RegisterType<MyShowsServiceMappingProfile>()
                .As<Profile>()
                .InstancePerLifetimeScope();
        }
    }
}
