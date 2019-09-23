using Autofac;
using AutoMapper;
using ShowMustNotGoOn.DatabaseService.Entities;

namespace ShowMustNotGoOn.DatabaseService
{
    public class DatabaseRepositoryModule : Module
    {
        public string ConnectionString { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShowsDbContext>()
                .WithParameter("options", DatabaseContextOptionsFactory.Get(ConnectionString))
                .InstancePerLifetimeScope();

            builder.RegisterType<DatabaseRepository>()
                .AsImplementedInterfaces();

            builder.RegisterType<DatabaseRepositoryMappingProfile>()
                .As<Profile>()
                .InstancePerLifetimeScope();
        }
    }
}
