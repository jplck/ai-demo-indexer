
using Microsoft.SemanticKernel.Text;

namespace Company.Function {
    public class SimpleChunker : IChunker
    {
        public List<string>? Chunk(string content)
        {
            var lines = TextChunker.SplitPlainTextLines(content, 40);
            return lines;
        }
    }
}