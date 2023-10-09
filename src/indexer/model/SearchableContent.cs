using System.Collections.ObjectModel;
using Azure.Search.Documents.Indexes;
using Newtonsoft.Json;

namespace Company.Function {

    public partial class SearchableContent {
        
        [SimpleField(IsKey = true, IsFilterable = true)]
        public required string Id { get; set; }

        [SearchableField(IsFilterable = true)]
        public required string Content { get; set; }

        [SearchableField(IsFilterable = true)]
        public required string DocumentId { get; set; }

         [SearchableField(IsFilterable = true)]
        public required string DocumentUri { get; set; }

        [SearchableField(VectorSearchDimensions = "1536", VectorSearchConfiguration = "vector-config")]
        [JsonIgnore]
        public Collection<float>? Embedding { get; set; }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }



}