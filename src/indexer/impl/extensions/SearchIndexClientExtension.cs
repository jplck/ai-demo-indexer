using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace Extensions {
    public static class SearchIndexClientExtension {
        public static SearchIndex? TryGetIndex(this SearchIndexClient searchIndexClient, string indexName) {
            try {
                return searchIndexClient.GetIndex(indexName);
            } catch (Exception e) {
                Console.WriteLine($"Index {indexName} not found.");
                return null;
            }
        }
    }
}