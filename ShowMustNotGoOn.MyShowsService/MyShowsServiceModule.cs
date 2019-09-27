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
            if (string.IsNullOrEmpty(ProxyAddress))
            {
                builder.RegisterInstance(new RestClient(MyShowsApiUrl))
                    .As<IRestClient>();
            }
            else
            {
                builder.RegisterInstance(new RestClient(MyShowsApiUrl)
                    {
                        Proxy = new WebProxy(ProxyAddress)
                        {
                            UseDefaultCredentials = true
                        }
                    })
                    .As<IRestClient>();
            }

            builder.RegisterType<MyShowsService>()
                .As<IMyShowsService>();

            builder.RegisterType<MyShowsServiceMappingProfile>()
                .As<Profile>()
                .InstancePerLifetimeScope();
        }
    }
}
