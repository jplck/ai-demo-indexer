using Azure.AI.OpenAI;
using Azure.Identity;
using Company.Function;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) => {
        services.AddSingleton<IEmbeddingsGenerator, AzureOpenAIEmbeddingsGenerator>();
        services.AddSingleton<IChunker, SimpleChunker>();
        services.AddSingleton<IDocumentRecognizer, DocumentIntelligence>();
        services.AddSingleton<ISearch, AzureSearch>();
        services.AddSingleton(new OpenAIClient(new Uri(configuration["OPENAI_API_ENDPOINT"]), new DefaultAzureCredential()));
    })
    .ConfigureAppConfiguration((context, config) => {
        config.AddConfiguration(configuration);
    })
    .Build();

host.Run();
