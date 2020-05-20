namespace ShowMustNotGoOn.Core.TelegramModel
{
    /// <summary>
    /// Telegram Message class
    /// </summary>
    public sealed class Message : Update
    {
        /// <summary>
        /// Unique message identifier
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// Optional. For text messages, the actual UTF-8 text of the message
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Is message is command
        /// </summary>
        public bool IsCommand() => Text.Trim().StartsWith("/");
    }
}
