using Autofac;
using ShowMustNotGoOn.Core;

namespace ShowMustNotGoOn.UsersService
{
    public class UsersServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UsersService>()
                .As<IUsersService>();
        }
    }
}
