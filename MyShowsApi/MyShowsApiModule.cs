using Autofac;
using RestSharp;

namespace ShowMustNotGoOn.MyShowsApi
{
    public class MyShowsApiModule : Module
    {
        public string MyShowsApiUrl { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new RestClient(MyShowsApiUrl)).As<IRestClient>();
            builder.RegisterType<MyShowsApi>();
        }
    }
}
