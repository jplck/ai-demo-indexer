using System.Collections.ObjectModel;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Company.Function {

    public class AzureOpenAIEmbeddingsGenerator : IEmbeddingsGenerator {

        private readonly IConfiguration _configuration;

        private OpenAIClient _openAIClient;

        const string DEPLOYMENT_NAME = "embedding";

        public AzureOpenAIEmbeddingsGenerator(IConfiguration configuration, OpenAIClient openAIClient) {
            _configuration = configuration;
            _openAIClient = openAIClient;
        }

        public async Task<IReadOnlyList<EnrichedChunk>> GenerateEmbeddingsAsync(List<string> chunks)
        {
            var enrichedChunks = new List<EnrichedChunk>();
            var chunkBlocks = (int)Math.Ceiling(chunks.Count / 16.0); //Current service limitation is 16 chunks per request
            
            for (int i = 0; i < chunkBlocks; i++)
            {
                var chunkBlock = chunks.Skip(i * 16).Take(16).ToList();
                
                EmbeddingsOptions options = new (chunkBlock);
                var results = await _openAIClient.GetEmbeddingsAsync(DEPLOYMENT_NAME, options);

                foreach (EmbeddingItem embeddingItem in results.Value.Data)
                {
                    var coll = new Collection<float>(embeddingItem.Embedding.ToList()); //TODO: Find a better way to do this
                    var enrichedChunk = new EnrichedChunk(
                        chunks[embeddingItem.Index],
                        new GenericEmbeddingItem() { Embedding = coll, Index = embeddingItem.Index }
                    );
                    
                    enrichedChunks.Add(enrichedChunk);
                }

            }
            return enrichedChunks;
        }
    }

}