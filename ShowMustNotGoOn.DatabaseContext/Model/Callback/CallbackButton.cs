namespace ShowMustNotGoOn.DatabaseContext.Model.Callback
{
    public sealed class CallbackButton
    {
        public BotMessage Message { get; set; }
        public string CallbackId { get; set; }
        public string CallbackData { get; set; }
    }
}
