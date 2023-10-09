namespace Company.Function {
    public interface IChunker {
        IReadOnlyCollection<Chunk>? Chunk(string documentUri, string content, int sizeOfChunk = 256);
    }
}