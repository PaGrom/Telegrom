﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RestSharp;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.MyShowsService.Model;

namespace ShowMustNotGoOn.MyShowsService
{
    public sealed class MyShowsRepository : ITvShowsRepository
    {
        private readonly IRestClient _client;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public MyShowsRepository(IRestClient client, IMapper mapper, ILogger logger)
        {
            _client = client;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name)
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

            var responseResult = await ExecuteAsync<ResponseResult>(request);

            return _mapper.Map<List<Result>, IEnumerable<TvShow>>(responseResult.Result);
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
