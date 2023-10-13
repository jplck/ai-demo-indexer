using System.Collections.ObjectModel;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Extensions;
using Microsoft.Extensions.Configuration;

namespace Company.Function {
    public class SearchOptionsFactory {
        public readonly IConfiguration _configuration;
        public SearchOptionsFactory(IConfiguration configuration) {
            _configuration = configuration;
        }

        public SearchOptions Create(Collection<float>? vector) {

            var searchOptions = new SearchOptions()
            {
                Size = 8,
                QueryLanguage = QueryLanguage.EnUs,
                SemanticConfigurationName = _configuration.TryGet("SEMANTIC_CONFIG_NAME"),
                QueryCaption = QueryCaptionType.Extractive,
                QueryAnswer = QueryAnswerType.Extractive,
                QueryCaptionHighlightEnabled = true,
                Select = { "Id", "DocumentId", "DocumentUri", "Content" }
            };

            if (vector is not null) {
                searchOptions.Vectors.Add(new SearchQueryVector {
                    Value = vector,
                    KNearestNeighborsCount = 3,
                    Fields = { "Embedding" }
                });
                searchOptions.QueryType = SearchQueryType.Semantic;
                return searchOptions;
            }
            else {
                searchOptions.QueryType = SearchQueryType.Simple;
            }
            return searchOptions;
        }
    }
}