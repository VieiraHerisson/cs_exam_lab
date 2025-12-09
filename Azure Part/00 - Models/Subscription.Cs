using System.Text.Json.Serialization;

namespace FeedbackPlatform.Models;

public class Subscription
{
    // Unique identifier for the subscription tier
    [JsonPropertyName("id")]
    public int Id { get; set; }

    // Subscription type: "Basic", "Premium", or "Enterprise"
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    // Price per feedback message for this subscription tier
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}