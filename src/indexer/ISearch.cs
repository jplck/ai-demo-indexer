namespace Company.Function {
    public interface ISearch {
        Task<string> SearchAsync(string query);

        Task AddDocumentAsync(DocumentRef docRef, IReadOnlyCollection<EnrichedChunk> chunks);
    }
}