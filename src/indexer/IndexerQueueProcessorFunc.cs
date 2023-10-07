using System.Net.Mime;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Text;
using Newtonsoft.Json;

namespace Company.Function
{
    public class IndexerQueueProcessorFunc
    {
        private readonly ILogger<IndexerQueueProcessorFunc> _logger;
        private readonly IConfiguration _configuration;

        private IEmbeddingsGenerator _embeddingsGenerator;

        private IChunker _chunker;

        private IDocumentRecognizer _documentRecognizer;

        private ISearch _search;

        public IndexerQueueProcessorFunc(ILogger<IndexerQueueProcessorFunc> logger, 
        IConfiguration configuration, 
        IEmbeddingsGenerator embeddingsGenerator, 
        IChunker chunker, 
        IDocumentRecognizer documentRecognizer,
        ISearch search)
        {
            _logger = logger;
            _configuration = configuration;
            _chunker = chunker;
            _documentRecognizer = documentRecognizer;
            _search = search;
            _embeddingsGenerator = embeddingsGenerator;
        }

        [Function(nameof(IndexerQueueProcessorFunc))]
        public async Task Run([ServiceBusTrigger("docsevents", Connection = "DOCUMENT_SERVICEBUS")] ServiceBusReceivedMessage message)
        {
            var blobEvent = JsonConvert.DeserializeObject<BlobCloudEvent>(message.Body.ToString());

            if (blobEvent is not null && blobEvent.Type == "Microsoft.Storage.BlobCreated")
            {
                var documentContent = await _documentRecognizer.RecognizeAsync(blobEvent.Data.Url);

                if (string.IsNullOrEmpty(documentContent)) {
                    _logger.LogDebug("documentContent is null or empty.");
                    return;
                }
                
                var chunks = _chunker.Chunk(documentContent);

                if (chunks is null) {
                    _logger.LogDebug("chunks is null.");
                    return;
                }
                
                var embeddings = await _embeddingsGenerator.GenerateEmbeddingsAsync(chunks);
                _logger.LogDebug("embeddings generated.");
            }
        }
    }
}
