namespace ShowMustNotGoOn.Core.Model
{
    public sealed class Message
    {
        public User FromUser { get; set; }

        public int MessageId { get; set; }

        public string Text { get; set; }

        public BotCommandType? BotCommand { get; set; }
    }
}
