using System.Text.Json.Serialization;

namespace FeedbackPlatform.Models;

public class Company
{
    // Unique identifier for the company
    [JsonPropertyName("id")]
    public int Id { get; set; }

    // Company name for display purposes
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    // Foreign key to subscription - we need to look up subscription details separately
    [JsonPropertyName("subscriptionId")]
    public int SubscriptionId { get; set; }
}