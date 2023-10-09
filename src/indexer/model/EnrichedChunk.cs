using System.Collections.ObjectModel;

namespace Company.Function {
    public class EnrichedChunk {
        public string Chunk { get; set;}

        public Collection<float> Embeddings { get; set;}

        public EnrichedChunk(string chunk, Collection<float> embeddings) {
            Chunk = chunk;
            Embeddings = embeddings;
        }
    }
}