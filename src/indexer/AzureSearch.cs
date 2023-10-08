
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

        private string _indexName;

        public AzureSearch(IConfiguration configuration, OpenAIClient openAIClient, IEmbeddingsGenerator embeddingsGenerator)
        {
            _configuration = configuration;
            _openAIClient = openAIClient;
            _embeddingsGenerator = embeddingsGenerator;

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

            string results = "";
            foreach (SearchResult<SearchableContent> result in searchResponse.Value.GetResults())
            {
                Console.WriteLine($"DocumentId: {result.Document.DocumentId}");
                Console.WriteLine($"DocumentUri: {result.Document.DocumentUri}");
                Console.WriteLine($"Content: {result.Document.Content}");
                Console.WriteLine($"Score: {result.Score}");
                Console.WriteLine();
                results += result.Document.Content + "\n";
            }

            var kernel = new KernelBuilder()
            .WithAzureChatCompletionService(_configuration["OPENAI_DEPLOYED_MODEL_NAME"], _configuration["OPENAI_API_ENDPOINT"], new DefaultAzureCredential())
            .Build();

            var chat = kernel.GetService<IChatCompletion>();
            var answerChat = chat.CreateNewChat($@"You are a system assistant who helps the user to get aggregated answers from his documents. Be brief in your answers");
            answerChat.AddUserMessage(@$" ## Question ##
            {query}
            ## End ##");
            answerChat.AddUserMessage(@$" ## Source ##
            {results}
            ## End ##

            You answer needs to be a json object with the following format.
            {{
                ""answer"": // the answer to the question, add a source reference to the end of each sentence. e.g. Apple is a fruit [reference1.pdf]. If no source available, put the answer as I don't know.
                ""thoughts"": // brief thoughts on how you came up with the answer, e.g. what sources you used, what you thought about, etc.
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
            var answerObject = JsonSerializer.Deserialize<JsonElement>(answerJson);
            var ans = answerObject.GetProperty("answer").GetString() ?? throw new InvalidOperationException("Failed to get answer");
            var thoughts = answerObject.GetProperty("thoughts").GetString() ?? throw new InvalidOperationException("Failed to get thoughts");
            return ans;
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