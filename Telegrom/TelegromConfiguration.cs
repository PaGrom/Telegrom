using Telegrom.Core.Configuration;

namespace Telegrom
{
    public class TelegromConfiguration : ITelegromConfiguration
    {
        public static ITelegromConfiguration Configuration { get; } = new TelegromConfiguration();
    }
}
