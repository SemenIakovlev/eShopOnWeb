using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var config = ctx.Configuration;

        var endpoint = config["COSMOS_ENDPOINT"] ?? "https://eshopcosmos-epam.documents.azure.com:443/";
        var databaseName = config["COSMOS_DATABASE"] ?? "ordersdb";
        var containerName = config["COSMOS_CONTAINER"] ?? "orders";
        Environment.SetEnvironmentVariable("AZURE_IDENTITY_LOGGING_ENABLED", "true");
        TokenCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());
        var cosmosClientOptions = new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            MaxRetryAttemptsOnRateLimitedRequests = 9,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };

        var cosmosClient = new CosmosClient(endpoint, credential, cosmosClientOptions);
        services.AddSingleton(cosmosClient);
        services.AddSingleton(provider => cosmosClient.GetDatabase(databaseName));
        services.AddSingleton(provider => cosmosClient.GetContainer(databaseName, containerName));
    })
    .Build();

await host.RunAsync();
