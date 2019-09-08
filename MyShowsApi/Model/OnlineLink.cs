using Newtonsoft.Json;

namespace ShowMustNotGoOn.MyShowsApi.Model
{
    public sealed class OnlineLink
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
