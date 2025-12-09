// Response DTO for the GetPriceOverview endpoint
// Returns calculated pricing and rating statistics per company

using System.Text.Json.Serialization;

namespace FeedbackPlatform.Models;

public class PriceOverviewResponse
{
    // Name of the company (from external API)
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = string.Empty;

    // Total cost for processing all feedback for this company
    // Calculated as: number of feedbacks × pricePerMessage
    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice { get; set; }

    // Average rating across all feedback for this company
    [JsonPropertyName("averageRating")]
    public decimal AverageRating { get; set; }
}