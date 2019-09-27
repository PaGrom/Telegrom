using Autofac;

namespace ShowMustNotGoOn.DatabaseContext
{
    public class DatabaseContextModule : Module
    {
        public string ConnectionString { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContext>()
                .WithParameter("options", DatabaseContextOptionsFactory.Get(ConnectionString))
                .InstancePerLifetimeScope();
        }
    }
}
