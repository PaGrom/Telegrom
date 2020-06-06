using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using RestSharp;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.MyShowsService.Model;

namespace ShowMustNotGoOn.MyShowsService
{
    public sealed class MyShowsService : IMyShowsService
    {
        private readonly IRestClient _client;
        private readonly IMapper _mapper;
        private readonly ILogger<MyShowsService> _logger;

        public MyShowsService(IRestClient client, IMapper mapper, ILogger<MyShowsService> logger)
        {
            _client = client;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name, CancellationToken cancellationToken)
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
                    query = name
                },
                id = 1
            });

            var responseResult = await ExecuteAsync<ResponseResult>(request, cancellationToken);

            return _mapper.Map<List<Result>, IEnumerable<TvShow>>(responseResult.Result);
        }

        public async Task<TvShow> GetTvShowAsync(int tvShowId, CancellationToken cancellationToken)
        {
            var request = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddJsonBody(new
            {
                jsonrpc = "2.0",
                method = "shows.GetById",
                @params = new
                {
                    showId = tvShowId,
                    withEpisodes = true
                },
                id = 1
            });

            var responseResult = await ExecuteAsync<ResponseResult>(request, cancellationToken);

            return _mapper.Map<Result, TvShow>(responseResult.Result.First());
        }

        private async Task<T> ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken) where T : new()
        {
            var response = await _client.ExecutePostAsync<T>(request, cancellationToken);

            if (response.ErrorException == null)
            {
                return response.Data;
            }
            const string message = "Error retrieving response.  Check inner details for more info.";
            var ex = new Exception(message, response.ErrorException);
            throw ex;
        }
    }
}
