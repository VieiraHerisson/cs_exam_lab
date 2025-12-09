namespace FeedbackPlatform.Repositories;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

public interface IBlobRepository
{
    // Appends a line of text to a blob (creates blob if doesn't exist)
    Task AppendToBlobAsync(string blobName, string content);

    // Reads the current content of a blob - empty if doesn't exist
    Task<string> ReadBlobAsync(string blobName);

    // Checks if a blob exists in the container
    Task<bool> BlobExistsAsync(string blobName);
}

public class BlobRepository : IBlobRepository
{
    // Container client for blob operations
    private readonly BlobContainerClient _containerClient;

    // Constructor receives BlobContainerClient via Dependency Injection
    public BlobRepository(BlobContainerClient containerClient)
    {
        _containerClient = containerClient;
    }

    // Appends content to a blob, creating it if it doesn't exist
    public async Task AppendToBlobAsync(string blobName, string content)
    {
        // Ensure container exists (defensive programming)
        await _containerClient.CreateIfNotExistsAsync();

        // Get reference to the specific blob
        var blobClient = _containerClient.GetBlobClient(blobName);

        // Read existing content if blob exists
        string existingContent = "";
        if (await blobClient.ExistsAsync())
        {
            // Download current blob content
            var downloadResult = await blobClient.DownloadContentAsync();
            existingContent = downloadResult.Value.Content.ToString();
        }

        // Combine existing content with new content
        string newContent = existingContent + content;

        // Convert to bytes for upload
        var bytes = Encoding.UTF8.GetBytes(newContent);

        // Upload with overwrite option (replaces entire blob)
        // Using MemoryStream to provide the byte content
        using var stream = new MemoryStream(bytes);
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    // Reads the entire content of a blob as a string
    // Returns empty string if blob doesn't exist
    public async Task<string> ReadBlobAsync(string blobName)
    {
        // Get reference to the blob
        var blobClient = _containerClient.GetBlobClient(blobName);

        // Check if blob exists before reading
        if (!await blobClient.ExistsAsync())
        {
            return string.Empty;
        }

        // Download and return the content as string
        var downloadResult = await blobClient.DownloadContentAsync();
        return downloadResult.Value.Content.ToString();
    }

    // Checks if a specific blob exists in the container
    public async Task<bool> BlobExistsAsync(string blobName)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.ExistsAsync();
        return response.Value;
    }
}