namespace ShowMustNotGoOn.DatabaseContext.Model
{
    public sealed class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public int TvShowId { get; set; }
    }
}
