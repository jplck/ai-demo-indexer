using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public class IndexerQueueProcessorFunc
    {
        private readonly ILogger<IndexerQueueProcessorFunc> _logger;
        private readonly IConfiguration _configuration;

        private DocumentAnalysisClient _diClient;

        public IndexerQueueProcessorFunc(ILogger<IndexerQueueProcessorFunc> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var diEndpoint = _configuration["DI_ENDPOINT"];

            if (string.IsNullOrEmpty(diEndpoint)) {
                throw new ArgumentNullException("Document Analysis endpoint must be provided.");
            }

            _diClient = new DocumentAnalysisClient(
                new Uri(diEndpoint),
                new DefaultAzureCredential()
            );

        }

        [Function(nameof(IndexerQueueProcessorFunc))]
        public async Task Run([ServiceBusTrigger("docsevents", Connection = "sbaidemo_SERVICEBUS")] ServiceBusReceivedMessage message)
        {
            var blobEvent = JsonConvert.DeserializeObject<BlobCloudEvent>(message.Body.ToString());

            if (blobEvent is not null && blobEvent.Type == "Microsoft.Storage.BlobCreated")
            {
                var blobUri = new Uri(blobEvent.Data.Url);
                var blobClient = new BlobClient(blobUri, new DefaultAzureCredential());
                
                AnalyzeDocumentOperation operation = await _diClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-layout", blobClient.Uri);

                AnalyzeResult result = operation.Value;
            }
        }
    }
}
