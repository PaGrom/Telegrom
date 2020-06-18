namespace Telegrom.Core.TelegramModel
{
    /// <summary>
    /// Telegram Update base class
    /// </summary>
    public abstract class Update
    {
        public int UpdateId { get; set; }
        public User From { get; set; }
    }
}
