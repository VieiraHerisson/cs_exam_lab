using Azure.Storage.Queues;

namespace FeedbackPlatform.Repositories;

public interface IQueueRepository
{
    // Sends a message to the follow-up queue
    Task SendMessageAsync(string messageContent);
}

public class QueueRepository : IQueueRepository
{
    // Queue client for sending messages
    private readonly QueueClient _queueClient;

    public QueueRepository(QueueClient queueClient)
    {
        _queueClient = queueClient;
    }

    // Sends a message to the Azure Queue
    public async Task SendMessageAsync(string messageContent)
    {
        // Ensure the queue exists (creates if not present)
        await _queueClient.CreateIfNotExistsAsync();

        // SendMessageAsync adds the message to the queue
        await _queueClient.SendMessageAsync(messageContent);
    }
}