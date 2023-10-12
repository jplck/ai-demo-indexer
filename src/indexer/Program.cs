using Azure.Identity;
using Azure.Search.Documents;
using Company.Function;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) => {
        services.AddSingleton<IEmbeddingsGenerator, AzureOpenAIEmbeddingsGenerator>();
        services.AddSingleton<IChunker, SimpleChunker>();
        services.AddSingleton<ISearch, AzureSearch>();
        services.AddSingleton<SearchOptionsFactory>();
        services.AddSingleton(new KernelBuilder()
            .WithAzureChatCompletionService(configuration["OPENAI_DEPLOYMENT_NAME"], configuration["OPENAI_API_ENDPOINT"], new DefaultAzureCredential())
            .WithAzureTextEmbeddingGenerationService(configuration["OPENAI_EMBEDDINGS_DEPLOYMENT_NAME"], configuration["OPENAI_API_ENDPOINT"], new DefaultAzureCredential())
            .Build());
    })
    .ConfigureAppConfiguration((context, config) => {
        config.AddConfiguration(configuration);
    })
    .Build();

host.Run();
