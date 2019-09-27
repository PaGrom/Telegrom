using Autofac;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.MyShowsService;

namespace ShowMustNotGoOn.TvShowsService
{
    public class TvShowsServiceModule : Module
    {
        public string MyShowsApiUrl { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TvShowsService>()
                .As<ITvShowsService>();

            builder.RegisterModule(new MyShowsServiceModule
            {
                MyShowsApiUrl = MyShowsApiUrl
            });
        }
    }
}
