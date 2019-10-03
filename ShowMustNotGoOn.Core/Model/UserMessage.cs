namespace ShowMustNotGoOn.Core.Model
{
    public sealed class UserMessage
    {
        public User User { get; set; }

        public int MessageId { get; set; }

        public string Text { get; set; }

        public BotCommandType? BotCommand { get; set; }
    }
}
