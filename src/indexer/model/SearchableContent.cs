using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using Azure.Search.Documents.Indexes;
using Newtonsoft.Json;

namespace Company.Function {

    public partial class SearchableContent {
        
        [SimpleField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; }

        [SearchableField(IsFilterable = true)]
        public string Content { get; set; }

        [SearchableField(IsFilterable = true)]
        public string DocumentId { get; set; }

         [SearchableField(IsFilterable = true)]
        public string DocumentUri { get; set; }

        [SearchableField(VectorSearchDimensions = "1536", VectorSearchConfiguration = "vector-config")]
        [JsonIgnore]
        public Collection<float>? Embedding { get; set; }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        public SearchableContent(Chunk chunk) {
            Id = Guid.NewGuid().ToString();
            Content = chunk.Content;
            DocumentId = chunk.DocumentId;
            DocumentUri = chunk.DocumentUri;
            Embedding = chunk.Embeddings;
        }
    }
}