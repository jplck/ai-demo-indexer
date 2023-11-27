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
            if (chunks == null || chunks.Count == 0)
            {
                throw new ArgumentException("Invalid input. The collection of chunks cannot be null or empty.");
            }

            var chunkBlocks = (int)Math.Ceiling(chunks.Count / 16.0); // Current service limitation is 16 chunks per request

            var listOfChunks = new List<Chunk>();

            for (int i = 0; i < chunkBlocks; i++)
            {
                var chunkBlock = chunks.Skip(i * 16).Take(16).ToList();

                try
                {
                    var results = await _embeddingGenerator.GenerateEmbeddingsAsync(chunkBlock.Select(c => c.Content).ToList());

                    for (int j = 0; j < chunkBlock.Count; j++)
                    {
                        var embedding = results[j]?.ToArray() ?? Array.Empty<float>();
                        chunkBlock[j].Embedding = new Collection<float>(embedding);
                    }

                    listOfChunks.AddRange(chunkBlock);
                }
                catch (Exception ex)
                {
                    // Handle the exception or log the error
                    Console.WriteLine($"Error generating embeddings for chunk block {i + 1}: {ex.Message}");
                }
            }

            return listOfChunks;
        }
    }

}