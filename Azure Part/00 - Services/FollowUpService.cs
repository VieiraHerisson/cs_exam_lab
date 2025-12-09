using FeedbackPlatform.Models;
using FeedbackPlatform.Repositories;
namespace FeedbackPlatform.Services;

public interface IFollowUpService
{
    // Processes a follow-up message from the queue
    // Creates or appends to CSV file in blob storage
    Task ProcessFollowUpAsync(FollowUpMessage message);
}

public class FollowUpService : IFollowUpService
{
    // Repository for blob storage operations
    private readonly IBlobRepository _blobRepository;

    // Constructor with blob repository injected
    public FollowUpService(IBlobRepository blobRepository)
    {
        _blobRepository = blobRepository;
    }

    // Processes a follow-up message by saving feedback to CSV blob
    // CSV format: UserName;Comments;Rating;Company;Subscription
    public async Task ProcessFollowUpAsync(FollowUpMessage message)
    {
        // Generate blob name based on company ID
        string blobName = $"feedback-{message.CompanyId}.csv";

        // Check if blob already exists to determine if header is needed
        bool blobExists = await _blobRepository.BlobExistsAsync(blobName);

        // Build the content to append
        string contentToAppend;

        if (!blobExists)
        {
            // Blob doesn't exist - add CSV header row first
            string header = "UserName;Comments;Rating;Company;Subscription";

            // Create the data row
            string dataRow = FormatCsvRow(message);

            // Combine header and data with newlines
            contentToAppend = header + Environment.NewLine + dataRow + Environment.NewLine;
        }
        else
        {
            // Blob exists - just append the new data row
            string dataRow = FormatCsvRow(message);
            contentToAppend = dataRow + Environment.NewLine;
        }

        // Append content to blob
        await _blobRepository.AppendToBlobAsync(blobName, contentToAppend);
    }

    // Helper method to format a single CSV row - Handles special characters that might break CSV parsing
    private string FormatCsvRow(FollowUpMessage message)
    {
        // Escape any semicolons in the data to prevent CSV parsing issues
        // Also escape newlines that could break row formatting
        string userName = EscapeCsvField(message.UserName);
        string comments = EscapeCsvField(message.Comments);
        string companyName = EscapeCsvField(message.CompanyName);
        string subscription = EscapeCsvField(message.Subscription);

        // Build CSV row with semicolon delimiter (as per requirements)
        // Format: UserName;Comments;Rating;Company;Subscription
        return $"{userName};{comments};{message.Rating};{companyName};{subscription}";
    }

    // Escapes special characters in CSV fields
    // Prevents data from breaking the CSV structure
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return string.Empty;
        }

        // Replace semicolons with commas to prevent column breaks
        // Replace newlines with spaces to prevent row breaks
        return field
            .Replace(";", ",")
            .Replace("\n", " ")
            .Replace("\r", " ");
    }
}