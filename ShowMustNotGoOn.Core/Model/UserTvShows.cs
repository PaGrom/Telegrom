namespace ShowMustNotGoOn.Core.Model
{
    public sealed class UserTvShows
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int TvShowId { get; set; }
        public TvShow TvShow { get; set; }
    }
}