using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;
using System.Text;

namespace Company.Function
{
    public class RecognizerFunc
    {
        private readonly ILogger<RecognizerFunc> _logger;
        private readonly IDocumentRecognizer _documentRecognizer;

        public RecognizerFunc(ILogger<RecognizerFunc> logger, IDocumentRecognizer documentRecognizer)
        {
            _logger = logger;
            _documentRecognizer = documentRecognizer;
        }

        [Function(nameof(RecognizerFunc))]
        [ServiceBusOutput("docscontent", Connection = "DOCUMENT_SERVICEBUS")]
        public async Task<string?> Run([ServiceBusTrigger("docsevents", Connection = "DOCUMENT_SERVICEBUS")] ServiceBusReceivedMessage message)
        {

            var blobEvent = JsonConvert.DeserializeObject<BlobCloudEvent>(message.Body.ToString());

            if (blobEvent is not null && blobEvent.Type == "Microsoft.Storage.BlobCreated")
            {
                var documentContent = await _documentRecognizer.RecognizeAsync(blobEvent.Data.Url);

                if (string.IsNullOrEmpty(documentContent))
                {
                    _logger.LogDebug("documentContent is null or empty.");
                }

                var content = new DocumentContentResult()
                {
                    DocumentUri = blobEvent.Data.Url,
                    Content = documentContent
                };

                return JsonConvert.SerializeObject(content);
            }
            return null;
        }
    }
}
