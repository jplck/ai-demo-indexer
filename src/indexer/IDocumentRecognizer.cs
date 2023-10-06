namespace Company.Function {
    public interface IDocumentRecognizer {
        Task<string> RecognizeAsync(string documentUri);
    }
}