using System.Net;
using Autofac;
using AutoMapper;
using RestSharp;
using ShowMustNotGoOn.Core;

namespace ShowMustNotGoOn.MyShowsService
{
    public class MyShowsServiceModule : Module
    {
        public string MyShowsApiUrl { get; set; }
        public string ProxyAddress { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(ctx =>
            {
                var restClient = new RestClient(MyShowsApiUrl);
                if (!string.IsNullOrEmpty(ProxyAddress))
                {
                    restClient.Proxy = new WebProxy(ProxyAddress)
                    {
                        UseDefaultCredentials = true
                    };
                }

                return restClient;
            }).As<IRestClient>();

            builder.RegisterType<MyShowsService>()
                .As<IMyShowsService>();

            builder.RegisterType<MyShowsServiceMappingProfile>()
                .As<Profile>()
                .InstancePerLifetimeScope();
        }
    }
}
