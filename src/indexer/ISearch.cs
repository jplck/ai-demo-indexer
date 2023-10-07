namespace Company.Function {
    public interface ISearch {
        Task SearchAsync(string query);

        Task AddDocumentAsync(DocumentRef docRef, IReadOnlyCollection<EnrichedChunk> chunks);
    }
}