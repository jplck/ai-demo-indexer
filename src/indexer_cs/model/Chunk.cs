using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using Azure.Search.Documents.Indexes;
using Newtonsoft.Json;

namespace Company.Function {

    public partial class Chunk {
        
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

        [SetsRequiredMembers]
        public Chunk(string content, string documentUri = "") {
            Id = Guid.NewGuid().ToString();
            Content = content;
            DocumentUri = Id;
            DocumentUri = documentUri;
            DocumentId = new Guid(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(documentUri))).ToString();
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}