namespace Telegrom.Core.Configuration
{
    public interface ITelegromConfiguration<out T> : ITelegromConfiguration
    {
        T Entry { get; }
    }

    public interface ITelegromConfiguration
    {
    }
}
