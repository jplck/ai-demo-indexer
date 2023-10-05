using System;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
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
                var blobServiceClient = new BlobServiceClient(new Uri($"{blobUri.Scheme}://{blobUri.Host}"), new DefaultAzureCredential());
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobUri.Segments[1].TrimEnd('/'));
                var blobClient = blobContainerClient.GetBlobClient(blobUri.Segments[2].TrimEnd('/'));
                /*
                var userDelegationKey = blobServiceClient.GetUserDelegationKey(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(15));

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = blobClient.BlobContainerName,
                    BlobName = blobClient.Name,
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                    Protocol = SasProtocol.Https,
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    Resource = "b",
                };
                sasBuilder.SetPermissions(BlobSasPermissions.All);

                var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
                {
                    Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, blobServiceClient.AccountName)
                };

                var uri = blobUriBuilder.ToUri();*/
                AnalyzeDocumentOperation operation = await _diClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-layout", blobClient.Uri);

                AnalyzeResult result = operation.Value;
            }
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        }
    }
}
