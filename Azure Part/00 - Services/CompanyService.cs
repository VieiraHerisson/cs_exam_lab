using FeedbackPlatform.Models;
using System.Text.Json;

namespace FeedbackPlatform.Services;
public interface ICompanyService
{
    // Retrieves company details by ID from external API
    // Returns null if company not found
    Task<Company?> GetCompanyByIdAsync(int companyId);

    // Retrieves all companies from external API
    Task<IEnumerable<Company>> GetAllCompaniesAsync();

    // Retrieves subscription details by ID from external API
    // Returns null if subscription not found
    Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId);

    // Retrieves all subscriptions from external API
    // Returns list of Basic, Premium, Enterprise tiers with pricing
    Task<IEnumerable<Subscription>> GetAllSubscriptionsAsync();
}


public class CompanyService : ICompanyService
{
    // HttpClient for making API requests
    private readonly HttpClient _httpClient;

    // JSON options for deserializing API responses
    private readonly JsonSerializerOptions _jsonOptions;

    // Constructor receives HttpClient via Dependency Injection
    public CompanyService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // Configure JSON deserialization options
        _jsonOptions = new JsonSerializerOptions
        {
            // Handle property names regardless of casing (camelCase, PascalCase)
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<Company?> GetCompanyByIdAsync(int companyId)
    {
        try
        {
            // Make GET request to the Companies endpoint
            var response = await _httpClient.GetAsync($"/companies/{companyId}");

            // Check if request was successful (200-299 status code)
            if (!response.IsSuccessStatusCode)
            {
                // Company not found or API error - return null
                return null;
            }

            // Read the response body as string
            var json = await response.Content.ReadAsStringAsync();

            // Deserialize JSON to Company object
            var company = JsonSerializer.Deserialize<Company>(json, _jsonOptions);

            return company;
        }
        catch (HttpRequestException)
        {
            // Network error or API unavailable
            return null;
        }
    }

    public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
    {
        try
        {
            // Make GET request to list all companies
            var response = await _httpClient.GetAsync("/companies");

            // Check if request was successful
            if (!response.IsSuccessStatusCode)
            {
                // Return empty list on error
                return Enumerable.Empty<Company>();
            }

            // Read and deserialize response
            var json = await response.Content.ReadAsStringAsync();
            var companies = JsonSerializer.Deserialize<IEnumerable<Company>>(json, _jsonOptions);

            // Return companies or empty list if deserialization failed
            return companies ?? Enumerable.Empty<Company>();
        }
        catch (HttpRequestException)
        {
            // Network error - return empty list
            return Enumerable.Empty<Company>();
        }
    }

    public async Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId)
    {
        try
        {
            // Make GET request to the Subscriptions endpoint
            var response = await _httpClient.GetAsync($"/subscriptions/{subscriptionId}");

            // Check if request was successful
            if (!response.IsSuccessStatusCode)
            {
                // Subscription not found or API error - return null
                return null;
            }

            // Read and deserialize response
            var json = await response.Content.ReadAsStringAsync();
            var subscription = JsonSerializer.Deserialize<Subscription>(json, _jsonOptions);

            return subscription;
        }
        catch (HttpRequestException)
        {
            // Network error - return null
            return null;
        }
    }

    public async Task<IEnumerable<Subscription>> GetAllSubscriptionsAsync()
    {
        try
        {
            // Make GET request to list all subscriptions
            var response = await _httpClient.GetAsync("/subscriptions");

            // Check if request was successful
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<Subscription>();
            }

            // Read and deserialize response
            var json = await response.Content.ReadAsStringAsync();
            var subscriptions = JsonSerializer.Deserialize<IEnumerable<Subscription>>(json, _jsonOptions);

            return subscriptions ?? Enumerable.Empty<Subscription>();
        }
        catch (HttpRequestException)
        {
            return Enumerable.Empty<Subscription>();
        }
    }
}