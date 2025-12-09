using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FeedbackPlatform.Models;
using FeedbackPlatform.Services;
using System.Text.Json;

namespace FeedbackPlatform.Functions;

public class ProcessFollowUp
{
    private readonly ILogger<ProcessFollowUp> _logger;
    private readonly IFollowUpService _followUpService;

    public ProcessFollowUp(
        ILogger<ProcessFollowUp> logger,
        IFollowUpService followUpService)
    {
        _logger = logger;
        _followUpService = followUpService;
    }

    [Function("ProcessFollowUp")]
    public async Task Run(
        [QueueTrigger("%QueueName%", Connection = "StorageConnection")] string queueMessage)
    {
        _logger.LogInformation("ProcessFollowUp function triggered");

        try
        {
            // Deserialize queue message to FollowUpMessage object
            var followUpMessage = JsonSerializer.Deserialize<FollowUpMessage>(queueMessage, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate deserialization succeeded
            if (followUpMessage == null)
            {
                _logger.LogError("Failed to deserialize queue message: {Message}", queueMessage);
                // Throwing exception will cause message to be retried or moved to poison queue
                throw new InvalidOperationException("Failed to deserialize queue message");
            }

            _logger.LogInformation(
                "Processing follow-up for feedback. ID: {FeedbackId}, Company: {CompanyName}, Rating: {Rating}",
                followUpMessage.FeedbackId,
                followUpMessage.CompanyName,
                followUpMessage.Rating);

            // Process the follow-up through service layer
            await _followUpService.ProcessFollowUpAsync(followUpMessage);

            _logger.LogInformation(
                "Follow-up processed successfully. CSV updated for company ID: {CompanyId}",
                followUpMessage.CompanyId);
        }
        catch (JsonException ex)
        {
            // JSON parsing error - log and let it go to poison queue
            _logger.LogError(ex, "JSON deserialization error for message: {Message}", queueMessage);
            throw;
        }
        catch (Exception ex)
        {
            // Unexpected error - log and rethrow
            _logger.LogError(ex, "Error processing follow-up message");
            throw;
        }
    }
}