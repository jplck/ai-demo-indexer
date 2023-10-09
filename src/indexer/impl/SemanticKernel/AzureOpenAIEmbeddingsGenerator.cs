using System.Collections.ObjectModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.Embeddings;

namespace Company.Function {

    public class AzureOpenAIEmbeddingsGenerator : IEmbeddingsGenerator {

        private ITextEmbeddingGeneration _embeddingGenerator;

        private IKernel _kernel;

        public AzureOpenAIEmbeddingsGenerator(IKernel kernel) {
            _kernel = kernel;
            _embeddingGenerator = _kernel.GetService<ITextEmbeddingGeneration>();
        }

        public async Task<IReadOnlyList<EnrichedChunk>> GenerateEmbeddingsAsync(List<string> chunks)
        {
            var enrichedChunks = new List<EnrichedChunk>();
            var chunkBlocks = (int)Math.Ceiling(chunks.Count / 16.0); //Current service limitation is 16 chunks per request
            
            var idx = 0;

            for (int i = 0; i < chunkBlocks; i++)
            {
                var chunkBlock = chunks.Skip(i * 16).Take(16).ToList();                
                var results = await _embeddingGenerator.GenerateEmbeddingsAsync(chunkBlock);

                foreach (var embeddingItem in results)
                {
                    var coll = new Collection<float>(embeddingItem.ToArray()); //TODO: Find a better way to do this
                    var enrichedChunk = new EnrichedChunk(
                        chunks[idx],
                        coll
                    );
                    
                    enrichedChunks.Add(enrichedChunk);
                    idx++;
                }

            }
            return enrichedChunks;
        }
    }

}