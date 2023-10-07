namespace Company.Function {
    public class EnrichedChunk {
        public string Chunk { get; set;}

        public GenericEmbeddingItem Embeddings { get; set;}

        public EnrichedChunk(string chunk, GenericEmbeddingItem embeddings) {
            Chunk = chunk;
            Embeddings = embeddings;
        }
    }
}