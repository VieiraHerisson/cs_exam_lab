using FeedbackPlatform.Models;
using FeedbackPlatform.Repositories;
using System.Text.Json;

namespace FeedbackPlatform.Services;

public interface IFeedbackService
{
    // Processes new feedback submission
    // Saves to Cosmos DB and queues for follow-up if needed
    // Returns the saved feedback
    Task<Feedback> SubmitFeedbackAsync(FeedbackRequest request);

    // Calculates price overview for a company
    Task<PriceOverviewResponse?> GetPriceOverviewAsync(int companyId);
}

public class FeedbackService : IFeedbackService
{
    // Repository for Cosmos DB operations
    private readonly IFeedbackRepository _feedbackRepository;

    // Repository for queue operations
    private readonly IQueueRepository _queueRepository;

    // Service for external API calls
    private readonly ICompanyService _companyService;

    // Constructor with all dependencies injected
    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        IQueueRepository queueRepository,
        ICompanyService companyService)
    {
        _feedbackRepository = feedbackRepository;
        _queueRepository = queueRepository;
        _companyService = companyService;
    }

    // Processes a new feedback submission
    public async Task<Feedback> SubmitFeedbackAsync(FeedbackRequest request)
    {
        // Step 1: Get company information from external API
        var company = await _companyService.GetCompanyByIdAsync(request.CompanyId);

        // Validate that company exists
        if (company == null)
        {
            throw new ArgumentException($"Company with ID {request.CompanyId} not found");
        }

        // Step 2: Get subscription details using the company's subscriptionId
        var subscription = await _companyService.GetSubscriptionByIdAsync(company.SubscriptionId);

        // Validate that subscription exists
        if (subscription == null)
        {
            throw new ArgumentException($"Subscription with ID {company.SubscriptionId} not found");
        }

        // Step 3: Create Feedback entity from request DTO
        var feedback = new Feedback
        {
            // Explicitly set Id to ensure Cosmos DB receives it
            Id = Guid.NewGuid().ToString(),
            UserName = request.UserName,
            Comments = request.Comments,
            Rating = request.Rating,
            CompanyId = request.CompanyId,
            CreatedAt = DateTime.UtcNow
        };

        // Step 4: Save feedback to Cosmos DB
        var savedFeedback = await _feedbackRepository.CreateFeedbackAsync(feedback);

        // Step 5: Check if follow-up processing is needed
        bool needsFollowUp = ShouldQueueForFollowUp(request.Rating, subscription.Type);

        if (needsFollowUp)
        {
            // Create follow-up message with all needed data
            var followUpMessage = new FollowUpMessage
            {
                FeedbackId = savedFeedback.Id,
                UserName = savedFeedback.UserName,
                Comments = savedFeedback.Comments,
                Rating = savedFeedback.Rating,
                CompanyId = savedFeedback.CompanyId,
                CompanyName = company.Name,
                Subscription = subscription.Type  // Use subscription.Type
            };

            // Serialize message to JSON for queue
            var messageJson = JsonSerializer.Serialize(followUpMessage);

            // Send to queue - triggers QueueTrigger function
            await _queueRepository.SendMessageAsync(messageJson);
        }

        return savedFeedback;
    }

    // Calculates price overview for a specific company
    public async Task<PriceOverviewResponse?> GetPriceOverviewAsync(int companyId)
    {
        // Step 1: Get company information from external API
        var company = await _companyService.GetCompanyByIdAsync(companyId);

        // Return null if company doesn't exist
        if (company == null)
        {
            return null;
        }

        // Step 2: Get subscription details for pricing information
        var subscription = await _companyService.GetSubscriptionByIdAsync(company.SubscriptionId);

        // Return null if subscription doesn't exist
        if (subscription == null)
        {
            return null;
        }

        // Step 3: Get all feedback for this company from Cosmos DB
        var feedbacks = await _feedbackRepository.GetFeedbackByCompanyIdAsync(companyId);
        var feedbackList = feedbacks.ToList();

        // Handle case where company has no feedback
        if (feedbackList.Count == 0)
        {
            return new PriceOverviewResponse
            {
                CompanyName = company.Name,
                TotalPrice = 0,
                AverageRating = 0
            };
        }

        // Step 4: Calculate total price
        decimal totalPrice = feedbackList.Count * subscription.Price;

        // Step 5: Calculate average rating
        decimal averageRating = (decimal)feedbackList.Average(f => f.Rating);

        // Round to one decimal place as per requirements
        averageRating = Math.Round(averageRating, 1);

        // Step 6: Build and return response
        return new PriceOverviewResponse
        {
            CompanyName = company.Name,
            TotalPrice = totalPrice,
            AverageRating = averageRating
        };
    }

    // Helper method to determine if feedback needs follow-up
    private bool ShouldQueueForFollowUp(int rating, string subscriptionType)
    {
        // Business rule from requirements:
        bool isLowRating = rating < 3;
        bool isPremiumOrEnterprise = subscriptionType.Equals("Premium", StringComparison.OrdinalIgnoreCase) ||
                                      subscriptionType.Equals("Enterprise", StringComparison.OrdinalIgnoreCase);

        return isLowRating && isPremiumOrEnterprise;
    }
}