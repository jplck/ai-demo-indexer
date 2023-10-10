
using Microsoft.SemanticKernel.Text;

namespace Company.Function {
    public class SimpleChunker : IChunker
    {
        public IReadOnlyCollection<Chunk>? Chunk(string documentUri, string content, int sizeOfChunk = 1000)
        {
            var lines = TextChunker.SplitPlainTextLines(content, sizeOfChunk);
            return lines.Select((line, index) => new Chunk(line, documentUri)).ToList();
        }
    }
}