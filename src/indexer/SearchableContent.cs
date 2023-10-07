using Azure.Search.Documents.Indexes;

namespace Company.Function {

    public partial class SearchableContent {
        
        [SimpleField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; }

        [SearchableField(IsFilterable = true)]
        public string Content { get; set; }

        [SearchableField(IsFilterable = true)]
        public string DocumentId { get; set; }

        [SearchableField(IsFilterable = true)]
        public int ChunkIndex { get; set; }

    }

}