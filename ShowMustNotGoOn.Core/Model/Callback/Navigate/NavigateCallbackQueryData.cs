namespace ShowMustNotGoOn.Core.Model.Callback.Navigate
{
    public abstract class NavigateCallbackQueryData : CallbackQueryData
    {
        public string SearchPattern { get; set; }
        public int PageCount { get; set; }
    }
}
