using Autofac;
using AutoMapper;
using ShowMustNotGoOn.DatabaseContext.Entities;

namespace ShowMustNotGoOn.DatabaseContext
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

            builder.RegisterType<DatabaseContextMappingProfile>()
                .As<Profile>()
                .InstancePerLifetimeScope();
        }
    }
}
