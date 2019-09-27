namespace ShowMustNotGoOn.Settings
{
    public class AppSettings
    {
        public DatabaseSettings DatabaseSettings { get; set; }
        public TelegramSettings TelegramSettings { get; set; }
        public MyShowsSettings MyShowsSettings { get; set; }
        public GlobalSettings GlobalSettings { get; set; }
    }
}
