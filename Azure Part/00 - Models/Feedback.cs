using System.Text.Json.Serialization;

namespace FeedbackPlatform.Models;


public class Feedback
{
    // Unique identifier for the feedback document
    [JsonPropertyName("id")]
    public string Id { get; set; }

    // The name of the user submitting feedback
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    // The actual feedback text/comments from the user
    [JsonPropertyName("comments")]
    public string Comments { get; set; } = string.Empty;

    // Rating from 1-5 (low ratings trigger follow-up for Premium/Enterprise)
    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    // Foreign key linking to the company - also serves as partition key
    // Using int to match the external API's company ID format
    [JsonPropertyName("companyId")]
    public int CompanyId { get; set; }

    // Timestamp when feedback was submitted
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    // Parameterless constructor ensures Id is always generated
    public Feedback()
    {
        Id = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }
}