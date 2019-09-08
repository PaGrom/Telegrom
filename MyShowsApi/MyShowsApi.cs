using System;
using System.Threading.Tasks;
using RestSharp;
using Serilog;
using ShowMustNotGoOn.MyShowsApi.Model;

namespace ShowMustNotGoOn.MyShowsApi
{
    public sealed class MyShowsApi
    {
        private readonly IRestClient _client;
        private readonly ILogger _logger;

        public MyShowsApi(IRestClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ResponseResult> SearchShowAsync(string param)
        {
            var request = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddJsonBody(new
            {
                jsonrpc = "2.0",
                method = "shows.Search",
                @params = new
                {
                    query = param
                },
                id = 1
            });

            return await ExecuteAsync<ResponseResult>(request);
        }

        private async Task<T> ExecuteAsync<T>(IRestRequest request) where T : new()
        {
            var response = await _client.ExecutePostTaskAsync<T>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var ex = new Exception(message, response.ErrorException);
                throw ex;
            }
            return response.Data;
        }
    }
}
