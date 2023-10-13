using System.Collections.ObjectModel;
using Microsoft.VisualBasic;

namespace Company.Function {

    public interface IEmbeddingsGenerator
    {
        Task<IReadOnlyCollection<Chunk>> GenerateEmbeddingsAsync(IReadOnlyCollection<Chunk> chunks);

        Task<IReadOnlyCollection<Chunk>> GenerateEmbeddingsAsync(string chunk);
    }

}