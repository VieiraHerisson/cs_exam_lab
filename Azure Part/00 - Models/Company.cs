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

    // Subscription tier: "Basic", "Premium", or "Enterprise"
    // Premium and Enterprise customers with low ratings get follow-up processing
    [JsonPropertyName("subscription")]
    public string Subscription { get; set; } = string.Empty;

    // Price per feedback message based on subscription tier
    [JsonPropertyName("pricePerMessage")]
    public decimal PricePerMessage { get; set; }
}