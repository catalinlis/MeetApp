namespace Notification.Services.Interfaces;

public interface IConsumerQueueService{
    Task ReadMessage();
    Task InitAsync();
    Task CloseAsync();
}