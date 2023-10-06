namespace Company.Function {
    public interface IChunker {
        List<string>? Chunk(string content);
    }
}