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
        private const string INBOUND_SB_QUEUE = "docscontent";
        private readonly ILogger<IndexerQueueProcessorFunc> _logger;
        private readonly IConfiguration _configuration;

        private IEmbeddingsGenerator _embeddingsGenerator;

        private IChunker _chunker;

        private ISearch _search;

        public IndexerQueueProcessorFunc(
            ILogger<IndexerQueueProcessorFunc> logger,
            IConfiguration configuration,
            IEmbeddingsGenerator embeddingsGenerator,
            IChunker chunker,
            ISearch search
        )
        {
            _logger = logger;
            _configuration = configuration;
            _chunker = chunker;
            _search = search;
            _embeddingsGenerator = embeddingsGenerator;
        }

        [Function(nameof(IndexerQueueProcessorFunc))]
        public async Task Run(
            [ServiceBusTrigger(INBOUND_SB_QUEUE, Connection = "DOCUMENT_SERVICEBUS")] ServiceBusReceivedMessage message
        )
        {
            var documentContentResult = JsonConvert.DeserializeObject<DocumentContentResult>(message.Body.ToString());
            if (documentContentResult is null)
            {
                _logger.LogDebug("documentContentResult is null.");
                return;
            }

            var chunks = _chunker.Chunk(documentContentResult.DocumentUri, documentContentResult.Content, 2048);

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
