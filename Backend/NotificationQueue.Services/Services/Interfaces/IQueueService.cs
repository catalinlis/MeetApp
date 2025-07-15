
namespace NotificationQueue.Services.Interfaces;

public interface IQueueService{
    Task ReadMessage();
    Task WriteMessage(NotificationMessageQueue message);
    Task InitAsync();
    Task CloseAsync();
}