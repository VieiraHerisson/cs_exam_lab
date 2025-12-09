using System.Text.Json.Serialization;

namespace FeedbackPlatform.Models;

// DTO for the POST request body when submitting feedback
public class FeedbackRequest
{
    // Name of the person submitting feedback
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    // The feedback text/comments
    [JsonPropertyName("comments")]
    public string Comments { get; set; } = string.Empty;

    // Rating value (1-5 scale, ratings below 3 trigger follow-up)
    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    // The ID of the company receiving feedback
    [JsonPropertyName("companyId")]
    public int CompanyId { get; set; }
}