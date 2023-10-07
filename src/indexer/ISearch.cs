namespace Company.Function {
    public interface ISearch {
        Task SearchAsync(string query);

        Task AddDocumentAsync(string chunk);
    }
}