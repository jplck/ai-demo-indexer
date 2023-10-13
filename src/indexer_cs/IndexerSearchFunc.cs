using System.Net;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Company.Function
{
    public class IndexerSearchFunc
    {
        private readonly ILogger _logger;

        private readonly ISearch _search;

        private readonly IConfiguration _configuration;

        public IndexerSearchFunc(
            IConfiguration configuration, 
            ILoggerFactory loggerFactory, 
            ISearch search
        )
        {
            _logger = loggerFactory.CreateLogger<IndexerSearchFunc>();
            _search = search;
            _configuration = configuration;
        }

        [Function("search")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
        )
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            var userPrompt = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(userPrompt))
            {
                response.WriteString("Please provide a prompt.");
                return response;
            }

            var answer = await _search.SearchAsync(userPrompt);
            response.WriteString(answer);

            return response;
        }
    }
}
