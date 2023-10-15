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
        private const string INBOUND_SB_QUEUE = "docsevents";
        private readonly ILogger<IndexerQueueProcessorFunc> _logger;
        private readonly IConfiguration _configuration;

        private IEmbeddingsGenerator _embeddingsGenerator;

        private IChunker _chunker;

        private ISearch _search;

        private IDocumentRecognizer _documentRecognizer;

        public IndexerQueueProcessorFunc(
            ILogger<IndexerQueueProcessorFunc> logger,
            IConfiguration configuration,
            IEmbeddingsGenerator embeddingsGenerator,
            IChunker chunker,
            ISearch search,
            IDocumentRecognizer documentRecognizer
        )
        {
            _logger = logger;
            _configuration = configuration;
            _chunker = chunker;
            _search = search;
            _embeddingsGenerator = embeddingsGenerator;
            _documentRecognizer = documentRecognizer;
        }

        [Function(nameof(IndexerQueueProcessorFunc))]
        public async Task Run(
            [ServiceBusTrigger(INBOUND_SB_QUEUE, Connection = "DOCUMENT_SERVICEBUS")] ServiceBusReceivedMessage message
        )
        {
            var blobEvent = JsonConvert.DeserializeObject<BlobCloudEvent>(message.Body.ToString());

            if (blobEvent is not null && blobEvent.Type == "Microsoft.Storage.BlobCreated")
            {
                var blobUri = new Uri(blobEvent.Data.Url);

                var recognizedContent = string.Empty;
                
                if (blobUri.GetFileEnding() == "pdf")
                {
                    recognizedContent = await _documentRecognizer.RecognizeAsync(blobUri.AbsoluteUri);
                }

                if (recognizedContent is null)
                {
                    _logger.LogDebug("recognizedContent is null.");
                    return;
                }

                var chunks = _chunker.Chunk(blobUri.AbsoluteUri, recognizedContent, 2048);

                if (chunks is null)
                {
                    _logger.LogDebug("chunks is null.");
                    return;
                }

                var chunksWithEmbeddings = await _embeddingsGenerator.GenerateEmbeddingsAsync(chunks);
                await _search.AddDocumentAsync(chunksWithEmbeddings);

                _logger.LogDebug("embeddings generated.");
            }

        }
    }
}
