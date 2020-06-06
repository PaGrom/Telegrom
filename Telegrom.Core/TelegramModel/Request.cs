namespace Telegrom.Core.TelegramModel
{
    /// <summary>
    /// Telegram Request base class
    /// </summary>
    public abstract class Request
    {
        /// <summary>
        /// Unique identifier for the target chat or username of the target channel
        /// </summary>
        public int ChatId { get; protected set; }
    }
}
