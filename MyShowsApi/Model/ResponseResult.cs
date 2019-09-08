using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShowMustNotGoOn.MyShowsApi.Model
{
    public sealed class ResponseResult
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; }

        [JsonProperty("result")]
        public List<Result> Result { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
    }
}
