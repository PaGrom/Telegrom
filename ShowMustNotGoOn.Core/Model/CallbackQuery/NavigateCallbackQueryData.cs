namespace ShowMustNotGoOn.Core.Model.CallbackQuery
{
    public abstract class NavigateCallbackQueryData : CallbackQueryData
    {
        public string SearchPattern { get; set; }
        public int PageCount { get; set; }
    }
}
