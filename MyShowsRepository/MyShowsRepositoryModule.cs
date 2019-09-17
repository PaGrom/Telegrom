using Autofac;
using AutoMapper;
using RestSharp;
using ShowMustNotGoOn.Core;

namespace ShowMustNotGoOn.MyShowsRepository
{
    public class MyShowsRepositoryModule : Module
    {
        public string MyShowsApiUrl { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new RestClient(MyShowsApiUrl))
                .As<IRestClient>();

            builder.RegisterType<MyShowsRepository>()
                .As<ITvShowsRepository>();

            builder.RegisterType<MyShowsRepositoryMappingProfile>()
                .As<Profile>()
                .InstancePerLifetimeScope();
        }
    }
}
