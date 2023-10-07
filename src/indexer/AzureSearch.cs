
using Microsoft.Extensions.Configuration;
using Azure.Search.Documents;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Google.Protobuf.WellKnownTypes;
using Azure.Search.Documents.Models;

//https://github.com/Azure-Samples/azure-search-dotnet-samples/blob/main/quickstart-semantic-search/SemanticSearchQuickstart/Program.cs

namespace Company.Function {
    public class AzureSearch : ISearch
    {
        IConfiguration _configuration;

        SearchClient _searchClient;

        SearchIndexClient _searchIndexClient;

        private string _indexName;

        public AzureSearch(IConfiguration configuration) {
            _configuration = configuration;

            var searchEndpoint = _configuration["COGNITIVE_SEARCH_ENDPOINT"];
            _indexName = _configuration["COGNITIVE_SEARCH_INDEX_NAME"];

            if (string.IsNullOrEmpty(searchEndpoint)) {
                throw new ArgumentNullException("Search endpoint must be provided.");
            }

            _searchIndexClient = new SearchIndexClient(
                new Uri(searchEndpoint),
                new DefaultAzureCredential()
            );

            _searchClient = new SearchClient(
                new Uri(searchEndpoint),
                _indexName,
                new DefaultAzureCredential()
            );

            CreateIndex();
        }

        public Task SearchAsync(string query)
        {
            throw new NotImplementedException();
        }

        private void CreateIndex() {
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(SearchableContent));

            var definition = new SearchIndex(_indexName, searchFields);

            SemanticSettings semanticSettings = new SemanticSettings();
            semanticSettings.Configurations.Add(new SemanticConfiguration(
                _configuration["SEMANTIC_CONFIG_NAME"],
                new PrioritizedFields() {
                    ContentFields = {
                        new SemanticField {FieldName = "Content"}
                    }
                }
            ));

            var vectorConfig = new HnswVectorSearchAlgorithmConfiguration(_configuration["VECTOR_CONFIG_NAME"]);
            var vectorSearch = new VectorSearch();
          
            vectorSearch.AlgorithmConfigurations.Add(vectorConfig);

            definition.SemanticSettings = semanticSettings;
            definition.VectorSearch = vectorSearch;

            _searchIndexClient.CreateOrUpdateIndex(definition);
        }

        public async Task AddDocumentAsync(DocumentRef docRef, IReadOnlyCollection<EnrichedChunk> chunks)
        {
            IndexDocumentsBatch<SearchableContent> batch = IndexDocumentsBatch.Create<SearchableContent>();
            foreach (var chunk in chunks)
            {
                var item = new IndexDocumentsAction<SearchableContent>(IndexActionType.Upload, new SearchableContent() {
                    Content = chunk.Chunk,
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = docRef.DocumentId,
                    DocumentUri = docRef.DocumentUri,
                    Embedding = chunk.Embeddings.Embedding, //TODO: ugly
                });
                batch.Actions.Add(item);
            }
            
            try {
                await _searchClient.IndexDocumentsAsync(batch);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                throw new Exception("Failed to index documents.");
            }
        }
    }
}