using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using FeedbackPlatform.Services;
using System.Net;

namespace FeedbackPlatform.Functions;

public class GetPriceOverview
{
    private readonly ILogger<GetPriceOverview> _logger;
    private readonly IFeedbackService _feedbackService;

    public GetPriceOverview(
        ILogger<GetPriceOverview> logger,
        IFeedbackService feedbackService)
    {
        _logger = logger;
        _feedbackService = feedbackService;
    }

    [Function("GetPriceOverview")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetPriceOverview/{companyId}")] HttpRequestData req,
        int companyId)
    {
        _logger.LogInformation("GetPriceOverview function triggered for company ID: {CompanyId}", companyId);

        try
        {
            // Validate companyId parameter
            if (companyId <= 0)
            {
                _logger.LogWarning("Invalid company ID: {CompanyId}", companyId);
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "companyId must be a positive number");
            }

            // Get price overview from service
            var priceOverview = await _feedbackService.GetPriceOverviewAsync(companyId);

            // Check if company was found
            if (priceOverview == null)
            {
                _logger.LogWarning("Company not found: {CompanyId}", companyId);
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, $"Company with ID {companyId} not found");
            }

            _logger.LogInformation(
                "Price overview retrieved. Company: {CompanyName}, Total: {TotalPrice}, Avg Rating: {AverageRating}",
                priceOverview.CompanyName,
                priceOverview.TotalPrice,
                priceOverview.AverageRating);

            // Return success response with price overview
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(priceOverview);

            return response;
        }
        catch (Exception ex)
        {
            // Unexpected error - log and return 500
            _logger.LogError(ex, "Error retrieving price overview for company {CompanyId}", companyId);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred while processing your request");
        }
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