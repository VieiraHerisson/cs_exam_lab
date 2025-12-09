using FeedbackPlatform.Models;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace FeedbackPlatform.Repositories;

public interface IFeedbackRepository
{
    // Saves a new feedback document to Cosmos DB
    // Returns the saved feedback with generated ID
    Task<Feedback> CreateFeedbackAsync(Feedback feedback);

    // Retrieves all feedback documents for a specific company
    // Uses partition key (companyId) for efficient querying
    Task<IEnumerable<Feedback>> GetFeedbackByCompanyIdAsync(int companyId);

    // Retrieves a single feedback by its ID and company ID
    // Both values needed because companyId is the partition key
    Task<Feedback?> GetFeedbackByIdAsync(string feedbackId, int companyId);
}

public class FeedbackRepository : IFeedbackRepository
{
    // Reference to the Cosmos DB container where feedbacks are stored
    private readonly Container _container;

    // Constructor receives CosmosClient via DI and gets reference to our container
    public FeedbackRepository(CosmosClient cosmosClient)
    {
        // Get reference to the specific container
        _container = cosmosClient.GetContainer("FeedbackDB", "Feedbacks");
    }

    // Creates a new feedback document in Cosmos DB
    public async Task<Feedback> CreateFeedbackAsync(Feedback feedback)
    {
        var response = await _container.CreateItemAsync(
            feedback,
            new PartitionKey(feedback.CompanyId)
        );

        // Return the created resource (includes any server-generated values)
        return response.Resource;
    }

    // Retrieves all feedback for a specific company using a SQL query
    public async Task<IEnumerable<Feedback>> GetFeedbackByCompanyIdAsync(int companyId)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.companyId = @companyId"
        ).WithParameter("@companyId", companyId);

        var queryOptions = new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(companyId)
        };

        // Execute the query and collect results
        var feedbacks = new List<Feedback>();

        // GetItemQueryIterator returns results in pages
        using var iterator = _container.GetItemQueryIterator<Feedback>(query, requestOptions: queryOptions);

        // Loop through all pages of results
        while (iterator.HasMoreResults)
        {
            // ReadNextAsync fetches the next page
            var response = await iterator.ReadNextAsync();

            // Add all items from this page to our list
            feedbacks.AddRange(response);
        }

        return feedbacks;
    }

    // Retrieves a single feedback document by ID
    public async Task<Feedback?> GetFeedbackByIdAsync(string feedbackId, int companyId)
    {
        try
        {
            // Requires both the document ID and partition key
            var response = await _container.ReadItemAsync<Feedback>(
                feedbackId,
                new PartitionKey(companyId)
            );

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Document doesn't exist - return null instead of throwing
            return null;
        }
    }
}