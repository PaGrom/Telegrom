using Newtonsoft.Json;

namespace ShowMustNotGoOn.MyShowsService.Model
{
    public sealed class Network
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
}
