using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using FeedbackPlatform.Repositories;
using FeedbackPlatform.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add Application Insights for monitoring and telemetry
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();


// Register CosmosClient as singleton (thread-safe, reuse connections)
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    // Get connection string from configuration
    var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");

    // Configure CosmosClient with custom serializer options
    // This ensures that [JsonPropertyName] attributes are respected
    var cosmosClientOptions = new CosmosClientOptions
    {
        // Use System.Text.Json serializer with proper settings
        SerializerOptions = new CosmosSerializationOptions
        {
            // Use camelCase for property names (matches our [JsonPropertyName] attributes)
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    // Create CosmosClient with connection string and options
    return new CosmosClient(connectionString, cosmosClientOptions);
});

builder.Services.AddSingleton<QueueClient>(serviceProvider =>
{
    // Get connection string and queue name from configuration
    var connectionString = Environment.GetEnvironmentVariable("StorageConnection");
    var queueName = Environment.GetEnvironmentVariable("QueueName");

    // Create QueueClient with message encoding option
    return new QueueClient(connectionString, queueName, new QueueClientOptions
    {
        MessageEncoding = QueueMessageEncoding.Base64
    });
});

// Register BlobContainerClient as singleton for blob operations
builder.Services.AddSingleton<BlobContainerClient>(serviceProvider =>
{
    // Get connection string and container name from configuration
    var connectionString = Environment.GetEnvironmentVariable("StorageConnection");
    var containerName = Environment.GetEnvironmentVariable("BlobContainerName");

    // Create BlobServiceClient first, then get container client
    var blobServiceClient = new BlobServiceClient(connectionString);
    return blobServiceClient.GetBlobContainerClient(containerName);
});

builder.Services.AddHttpClient<ICompanyService, CompanyService>(client =>
{
    // Get base URL from configuration
    var baseUrl = Environment.GetEnvironmentVariable("FeedbackApiBaseUrl");

    // Set base address for all requests made by this client
    client.BaseAddress = new Uri(baseUrl!);

    // Set default headers
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


// Feedback repository for Cosmos DB operations
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

// Queue repository for sending messages to Azure Queue
builder.Services.AddScoped<IQueueRepository, QueueRepository>();

// Blob repository for storing CSV files
builder.Services.AddScoped<IBlobRepository, BlobRepository>();

// Feedback service for processing feedback submissions and statistics
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

// Follow-up service for processing queue messages and creating CSVs
builder.Services.AddScoped<IFollowUpService, FollowUpService>();

// Build and run the application
builder.Build().Run();