// Message structure for the follow-up queue
// Contains all data needed to create the CSV blob entry

using System.Text.Json.Serialization;

namespace FeedbackPlatform.Models;

// Queue message containing feedback data for follow-up processing
public class FollowUpMessage
{
    // Reference to the original feedback document
    [JsonPropertyName("feedbackId")]
    public string FeedbackId { get; set; } = string.Empty;

    // User who submitted the feedback
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    // The feedback comments
    [JsonPropertyName("comments")]
    public string Comments { get; set; } = string.Empty;

    // The rating given (will be below 3)
    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    // Company ID for determining blob filename
    [JsonPropertyName("companyId")]
    public int CompanyId { get; set; }

    // Company name for the CSV content
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = string.Empty;

    // Subscription tier for the CSV content
    [JsonPropertyName("subscription")]
    public string Subscription { get; set; } = string.Empty;
}