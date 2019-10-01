namespace ShowMustNotGoOn.Core.Model
{
    public sealed class Subscription
    {
        public int Id { get; set; }
        public User User { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public TvShow TvShow { get; set; }
    }
}
