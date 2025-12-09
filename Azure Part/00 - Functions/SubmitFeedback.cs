using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using FeedbackPlatform.Models;
using FeedbackPlatform.Services;
using System.Net;
using System.Text.Json;

namespace FeedbackPlatform.Functions;

public class SubmitFeedback
{
    private readonly ILogger<SubmitFeedback> _logger;

    // Service for processing feedback business logic
    private readonly IFeedbackService _feedbackService;

    public SubmitFeedback(
        ILogger<SubmitFeedback> logger,
        IFeedbackService feedbackService)
    {
        _logger = logger;
        _feedbackService = feedbackService;
    }

    [Function("SubmitFeedback")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("SubmitFeedback function triggered");

        try
        {
            // Read and deserialize request body
            var requestBody = await req.ReadAsStringAsync();

            // Validate that body is not empty
            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Request body is empty");
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request body is required");
            }

            // Deserialize JSON to FeedbackRequest object
            var feedbackRequest = JsonSerializer.Deserialize<FeedbackRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate deserialization succeeded
            if (feedbackRequest == null)
            {
                _logger.LogWarning("Failed to deserialize request body");
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format");
            }

            // Validate required fields
            var validationError = ValidateFeedbackRequest(feedbackRequest);
            if (validationError != null)
            {
                _logger.LogWarning("Validation failed: {Error}", validationError);
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validationError);
            }

            // Process feedback through service layer
            var savedFeedback = await _feedbackService.SubmitFeedbackAsync(feedbackRequest);

            _logger.LogInformation(
                "Feedback submitted successfully. ID: {FeedbackId}, Company: {CompanyId}, Rating: {Rating}",
                savedFeedback.Id,
                savedFeedback.CompanyId,
                savedFeedback.Rating);

            // Return success response with saved feedback
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(savedFeedback);

            return response;
        }
        catch (ArgumentException ex)
        {
            // Company not found or invalid argument
            _logger.LogWarning("Argument error: {Message}", ex.Message);
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            // Unexpected error - log and return 500
            _logger.LogError(ex, "Error processing feedback submission");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred while processing your request");
        }
    }

    // Validates the feedback request fields
    private string? ValidateFeedbackRequest(FeedbackRequest request)
    {
        // Check userName is provided
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return "userName is required";
        }

        // Check comments is provided
        if (string.IsNullOrWhiteSpace(request.Comments))
        {
            return "comments is required";
        }

        // Check rating is in valid range (1-5)
        if (request.Rating < 1 || request.Rating > 5)
        {
            return "rating must be between 1 and 5";
        }

        // Check companyId is valid (positive number)
        if (request.CompanyId <= 0)
        {
            return "companyId must be a positive number";
        }

        // All validations passed
        return null;
    }

    // Helper method to create error responses
    private async Task<HttpResponseData> CreateErrorResponse(
        HttpRequestData req,
        HttpStatusCode statusCode,
        string message)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new { error = message });
        return response;
    }
}