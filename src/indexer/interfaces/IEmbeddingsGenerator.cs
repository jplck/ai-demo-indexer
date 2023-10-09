using System.Collections.ObjectModel;
using Microsoft.VisualBasic;

namespace Company.Function {

    public interface IEmbeddingsGenerator
    {
        Task<IReadOnlyList<EnrichedChunk>> GenerateEmbeddingsAsync(List<string> chunks);
    }

}