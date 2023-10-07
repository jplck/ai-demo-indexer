using System.Collections.ObjectModel;

namespace Company.Function
{
    public class GenericEmbeddingItem
    {
        public required Collection<float> Embedding { get; set; }
        public int Index { get; set; }
    }

}