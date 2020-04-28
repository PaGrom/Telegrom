namespace ShowMustNotGoOn.DatabaseContext.Model
{
    public sealed class User
    {
        public int Id { get; set; }
        public int TelegramId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
