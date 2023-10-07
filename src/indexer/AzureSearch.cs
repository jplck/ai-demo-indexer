
using Microsoft.Extensions.Configuration;
using Azure.Search.Documents;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Google.Protobuf.WellKnownTypes;

//https://github.com/Azure-Samples/azure-search-dotnet-samples/blob/main/quickstart-semantic-search/SemanticSearchQuickstart/Program.cs

namespace Company.Function {
    public class AzureSearch : ISearch
    {
        IConfiguration _configuration;

        SearchClient _searchClient;

        SearchIndexClient _searchIndexClient;

        const string IndexName = "cognitive-search";

        public AzureSearch(IConfiguration configuration) {
            _configuration = configuration;

            var searchEndpoint = _configuration["COGNITIVE_SEARCH_ENDPOINT"];

            if (string.IsNullOrEmpty(searchEndpoint)) {
                throw new ArgumentNullException("Search endpoint must be provided.");
            }

            _searchIndexClient = new SearchIndexClient(
                new Uri(searchEndpoint),
                new DefaultAzureCredential()
            );

            _searchClient = new SearchClient(
                new Uri(searchEndpoint),
                IndexName,
                new DefaultAzureCredential()
            );

            CreateIndex();
        }

        public Task SearchAsync(string query)
        {
            throw new NotImplementedException();
        }

        private void CreateIndex() {
            const string vectorConfigName = "vector-config";
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(SearchableContent));

            var vectorField = new SearchField("Embedding", SearchFieldDataType.Collection(SearchFieldDataType.Single))
            {
                VectorSearchDimensions = 1536,
                VectorSearchConfiguration = vectorConfigName
            };
            searchFields.Add(vectorField);

            var definition = new SearchIndex(IndexName, searchFields);

            SemanticSettings semanticSettings = new SemanticSettings();
            semanticSettings.Configurations.Add(new SemanticConfiguration(
                "semantic-config",
                new PrioritizedFields() {
                    ContentFields = {
                        new SemanticField {FieldName = "Content"}
                    }
                }
            ));

            var vectorConfig = new HnswVectorSearchAlgorithmConfiguration(vectorConfigName);
            var vectorSearch = new VectorSearch();
          
            vectorSearch.AlgorithmConfigurations.Add(vectorConfig);

            definition.SemanticSettings = semanticSettings;
            definition.VectorSearch = vectorSearch;

            _searchIndexClient.CreateOrUpdateIndex(definition);
        }

        public Task AddDocumentAsync(string chunk)
        {
            throw new NotImplementedException();
        }
    }
}