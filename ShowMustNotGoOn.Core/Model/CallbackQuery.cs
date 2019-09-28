namespace ShowMustNotGoOn.Core.Model
{
    public sealed class CallbackQuery
    {
        public string Id { get; set; }
        public User FromUser { get; set; }
        public Message Message { get; set; }
        public string InlineMessageId { get; set; }
        public string Data { get; set; }
    }
}
