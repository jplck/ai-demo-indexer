
using Microsoft.SemanticKernel.Text;

namespace Company.Function {
    public class SimpleChunker : IChunker
    {
        public List<string>? Chunk(string content, int sizeOfChunk = 256)
        {
            var lines = TextChunker.SplitPlainTextLines(content, sizeOfChunk);
            return lines;
        }
    }
}