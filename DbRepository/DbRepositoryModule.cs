using Autofac;
using AutoMapper;

namespace DbRepository
{
    public class DbRepositoryModule : Module
    {
        public string ConnectionString { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShowsDbContext>()
                .WithParameter("options", DbContextOptionsFactory.Get(ConnectionString))
                .InstancePerLifetimeScope();

            builder.RegisterType<ShowsDbRepository>()
                .AsImplementedInterfaces();

            builder.RegisterType<DbRepositoryMappingProfile>()
                .As<Profile>()
                .InstancePerLifetimeScope();
        }
    }
}
