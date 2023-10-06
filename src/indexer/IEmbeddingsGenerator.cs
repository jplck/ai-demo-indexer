namespace Company.Function {

    public class GenericEmbeddingItem {
        public IReadOnlyList<float>? Embedding { get; set; }
        public int Index { get; set; }
    }

    public interface IEmbeddingsGenerator
    {
        Task<IReadOnlyList<GenericEmbeddingItem>> GenerateEmbeddingsAsync(List<string> chunks);
    }

}