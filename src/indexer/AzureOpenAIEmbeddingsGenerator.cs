using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Company.Function {

    public class AzureOpenAIEmbeddingsGenerator : IEmbeddingsGenerator {

        private readonly IConfiguration _configuration;

        private OpenAIClient _openAIClient;

        public AzureOpenAIEmbeddingsGenerator(IConfiguration configuration) {
            _configuration = configuration;

            var oaiEndpoint = _configuration["OPENAI_API_ENDPOINT"];

            if (string.IsNullOrEmpty(oaiEndpoint)) {
                throw new ArgumentNullException("OpenAI endpoint must be provided.");
            }

            _openAIClient = new (new Uri(oaiEndpoint), new DefaultAzureCredential());
        }

        public async Task<IReadOnlyList<GenericEmbeddingItem>> GenerateEmbeddingsAsync(List<string> chunks)
        {
            EmbeddingsOptions options = new (chunks);
            var results = await _openAIClient.GetEmbeddingsAsync("embedding", options);

            var genericResults = new List<GenericEmbeddingItem>();

            foreach (EmbeddingItem embeddingItem in results.Value.Data)
            {
                genericResults.Add(new GenericEmbeddingItem() { Embedding = embeddingItem.Embedding, Index = embeddingItem.Index });
            }
            return genericResults;
        }
    }

}