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

        public async Task<IReadOnlyCollection<Chunk>> GenerateEmbeddingsAsync(string chunk) { return await GenerateEmbeddingsAsync(new ReadOnlyCollection<Chunk>(new List<Chunk> { new Chunk(chunk, string.Empty) })); }

        public async Task<IReadOnlyCollection<Chunk>> GenerateEmbeddingsAsync(IReadOnlyCollection<Chunk> chunks)
        {
            var chunkBlocks = (int)Math.Ceiling(chunks.Count / 16.0); //Current service limitation is 16 chunks per request
            
            var listOfChunks = new List<Chunk>();

            for (int i = 0; i < chunkBlocks; i++)
            {
                var chunkBlock = chunks.Skip(i * 16).Take(16).ToList();           
                var results = await _embeddingGenerator.GenerateEmbeddingsAsync(chunks.Select(c => c.Content).ToList());
                chunkBlock.ForEach(c => c.Embedding = new Collection<float>(results[chunkBlock.IndexOf(c)].ToArray()));
                listOfChunks.AddRange(chunkBlock);
            }
            return listOfChunks;
        }
    }

}