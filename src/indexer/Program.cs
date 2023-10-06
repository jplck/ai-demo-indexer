using Company.Function;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostContext, services) => {
        services.AddSingleton<IEmbeddingsGenerator, AzureOpenAIEmbeddingsGenerator>();
        services.AddSingleton<IChunker, SimpleChunker>();
        services.AddSingleton<IDocumentRecognizer, DocumentIntelligence>();
    })
    .Build();

host.Run();
