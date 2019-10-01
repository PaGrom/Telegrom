namespace ShowMustNotGoOn.Core.Model.Callback
{
    public sealed class CallbackQuery
    {
        public string Id { get; set; }
        public User FromUser { get; set; }
        public Message Message { get; set; }
        public int CallbackQueryDataId { get; set; }
        public CallbackQueryData CallbackQueryData { get; set; }
    }
}
