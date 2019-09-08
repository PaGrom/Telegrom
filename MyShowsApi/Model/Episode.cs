using Newtonsoft.Json;

namespace ShowMustNotGoOn.MyShowsApi.Model
{
    public sealed class Episode
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("showId")]
        public long ShowId { get; set; }

        [JsonProperty("seasonNumber")]
        public long SeasonNumber { get; set; }

        [JsonProperty("episodeNumber")]
        public long EpisodeNumber { get; set; }

        [JsonProperty("airDate")]
        public string AirDate { get; set; }

        [JsonProperty("airDateUTC")]
        public string AirDateUtc { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("commentsCount")]
        public long CommentsCount { get; set; }
    }
}
