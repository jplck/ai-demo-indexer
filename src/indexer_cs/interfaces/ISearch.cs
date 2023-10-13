namespace Company.Function {
    public interface ISearch {
        Task<string> SearchAsync(string query);

        Task AddDocumentAsync(IReadOnlyCollection<Chunk> chunks);
    }
}