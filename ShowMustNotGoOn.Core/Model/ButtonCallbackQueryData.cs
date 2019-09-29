namespace ShowMustNotGoOn.Core.Model
{
    public sealed class ButtonCallbackQueryData
    {
        public int Id { get; set; }
        public User User { get; set; }
        public string Data { get; set; }
    }
}
