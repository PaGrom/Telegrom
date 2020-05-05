namespace ShowMustNotGoOn.Core.TelegramModel
{
    /// <summary>
    /// Telegram Update base class
    /// </summary>
    public abstract class Update
    {
        public User From { get; set; }
    }
}
