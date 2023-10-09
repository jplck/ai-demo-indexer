
using Microsoft.Extensions.Configuration;
using Azure.Search.Documents;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCognitiveSearch;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using System.Text.Json;

//https://github.com/Azure-Samples/azure-search-dotnet-samples/blob/main/quickstart-semantic-search/SemanticSearchQuickstart/Program.cs

namespace Company.Function
{
    public class AzureSearch : ISearch
    {
        IConfiguration _configuration;

        SearchClient _searchClient;

        SearchIndexClient _searchIndexClient;

        OpenAIClient _openAIClient;

        IEmbeddingsGenerator _embeddingsGenerator;

        IKernel _kernel;

        private string _indexName;

        public AzureSearch(IConfiguration configuration, OpenAIClient openAIClient, IEmbeddingsGenerator embeddingsGenerator, IKernel kernel)
        {
            _configuration = configuration;
            _openAIClient = openAIClient;
            _embeddingsGenerator = embeddingsGenerator;
            _kernel = kernel;

            var searchEndpoint = _configuration["COGNITIVE_SEARCH_ENDPOINT"];
            _indexName = _configuration["COGNITIVE_SEARCH_INDEX_NAME"];

            if (string.IsNullOrEmpty(searchEndpoint))
            {
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

        public async Task<string> SearchAsync(string query)
        {
            var chat = _kernel.GetService<IChatCompletion>();
            var answerChat = chat.CreateNewChat($@"You are a system assistant who helps the user to get aggregated answers from his documents. Be brief in your answers");
            answerChat.AddUserMessage(@$" ## Question ##
            {query}
            ## End ##");

            var queryEmbeddings = await _embeddingsGenerator.GenerateEmbeddingsAsync(new List<string>() { query });

            var vector = queryEmbeddings[0].Embeddings.Embedding.ToList();
            var searchOptions = new SearchOptions()
            {
                Vectors = { new() { Value = vector, KNearestNeighborsCount = 3, Fields = { "Embedding" } } },
                Size = 3,
                QueryType = SearchQueryType.Semantic,
                QueryLanguage = QueryLanguage.EnUs,
                SemanticConfigurationName = _configuration["SEMANTIC_CONFIG_NAME"],
                QueryCaption = QueryCaptionType.Extractive,
                QueryAnswer = QueryAnswerType.Extractive,
                QueryCaptionHighlightEnabled = true,
                Select = { "Id", "DocumentId", "DocumentUri", "Content" }
            };

            var searchResponse = await _searchClient.SearchAsync<SearchableContent>(query, searchOptions);
            string results = string.Join("\n", searchResponse.Value.GetResults().Select(doc => doc.Document.ToString()).ToArray());
            
            answerChat.AddUserMessage(@$" ## Source ##
            {results}
            ## End ##

            You answer needs to be a valid json object with the following format and nothing more.
            {{
                ""answer"": // the answer to the question. If you have no answer you can return an no answer message.
                ""references"": // add the list of reference documents that are contained in the source DocumentUri field. Return empty list if nothing is found.
            }}");

            var answer = await chat.GetChatCompletionsAsync(
                answerChat,
                new ChatRequestSettings
                {
                    Temperature = 0.7,
                    MaxTokens = 256,
                },
                default
            );
            var answerJson = answer[0].ModelResult.GetOpenAIChatResult().Choice.Message.Content;
            return answerJson;
        }

        private void CreateIndex()
        {
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(SearchableContent));

            var definition = new SearchIndex(_indexName, searchFields);

            SemanticSettings semanticSettings = new SemanticSettings();
            semanticSettings.Configurations.Add(new SemanticConfiguration(
                _configuration["SEMANTIC_CONFIG_NAME"],
                new PrioritizedFields()
                {
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
                var item = new IndexDocumentsAction<SearchableContent>(IndexActionType.Upload, new SearchableContent()
                {
                    Content = chunk.Chunk,
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = docRef.DocumentId,
                    DocumentUri = docRef.DocumentUri,
                    Embedding = chunk.Embeddings.Embedding, //TODO: ugly
                });
                batch.Actions.Add(item);
            }

            try
            {
                await _searchClient.IndexDocumentsAsync(batch);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Failed to index documents.");
            }
        }
    }
}