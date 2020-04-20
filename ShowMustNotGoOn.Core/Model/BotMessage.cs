namespace ShowMustNotGoOn.Core.Model
{
    public sealed class BotMessage
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int MessageId { get; set; }

        public BotCommandType? BotCommandType { get; set; }

        public string SearchPattern { get; set; }

        public int CurrentShowId { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }
    }
}
