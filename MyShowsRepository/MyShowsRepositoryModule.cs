using Autofac;
using AutoMapper;
using RestSharp;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.MyShowsRepository.Model;

namespace ShowMustNotGoOn.MyShowsRepository
{
    public class MyShowsRepositoryModule : Module
    {
        public string MyShowsApiUrl { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new RestClient(MyShowsApiUrl)).As<IRestClient>();
            builder.RegisterType<MyShowsRepository>().As<ITvShowsRepository>();
            builder.Register(ctx =>
                    {
                        var config = new MapperConfiguration(cfg => {
                            cfg.CreateMap<Result, TvShow>();
                        });
                        return config.CreateMapper();
                    })
                .As<IMapper>()
                .InstancePerLifetimeScope();
        }
    }
}
