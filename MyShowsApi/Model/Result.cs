using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShowMustNotGoOn.MyShowsApi.Model
{
    public sealed class Result
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("titleOriginal")]
        public string TitleOriginal { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("totalSeasons")]
        public long TotalSeasons { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("started")]
        public string Started { get; set; }

        [JsonProperty("ended")]
        public string Ended { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("kinopoiskId")]
        public long KinopoiskId { get; set; }

        [JsonProperty("kinopoiskRating")]
        public long KinopoiskRating { get; set; }

        [JsonProperty("kinopoiskVoted")]
        public long KinopoiskVoted { get; set; }

        [JsonProperty("kinopoiskUrl")]
        public string KinopoiskUrl { get; set; }

        [JsonProperty("tvrageId")]
        public long TvrageId { get; set; }

        [JsonProperty("imdbId")]
        public string ImdbId { get; set; }

        [JsonProperty("imdbRating")]
        public long ImdbRating { get; set; }

        [JsonProperty("imdbVoted")]
        public long ImdbVoted { get; set; }

        [JsonProperty("imdbUrl")]
        public string ImdbUrl { get; set; }

        [JsonProperty("watching")]
        public long Watching { get; set; }

        [JsonProperty("watchingTotal")]
        public long WatchingTotal { get; set; }

        [JsonProperty("voted")]
        public long Voted { get; set; }

        [JsonProperty("rating")]
        public long Rating { get; set; }

        [JsonProperty("runtime")]
        public long Runtime { get; set; }

        [JsonProperty("runtimeTotal")]
        public string RuntimeTotal { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("genreIds")]
        public List<long> GenreIds { get; set; }

        [JsonProperty("network")]
        public Network Network { get; set; }

        [JsonProperty("episodes")]
        public List<Episode> Episodes { get; set; }

        [JsonProperty("onlineLinks")]
        public List<OnlineLink> OnlineLinks { get; set; }

        [JsonProperty("onlineLinkExclusive")]
        public OnlineLink OnlineLinkExclusive { get; set; }
    }
}
