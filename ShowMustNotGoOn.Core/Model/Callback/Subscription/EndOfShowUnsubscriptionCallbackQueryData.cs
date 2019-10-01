namespace ShowMustNotGoOn.Core.Model.Callback.Subscription
{
    public sealed class EndOfShowUnsubscriptionCallbackQueryData : SubscriptionCallbackQueryData
    {
        public EndOfShowUnsubscriptionCallbackQueryData()
        {
            CallbackQueryType = CallbackQueryType.UnsubscribeEndOfShow;
        }
    }
}
