using Azure.Search.Documents.Indexes;

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

        [SimpleField(IsFilterable = true)]
        public int ChunkIndex { get; set; }

    }

}