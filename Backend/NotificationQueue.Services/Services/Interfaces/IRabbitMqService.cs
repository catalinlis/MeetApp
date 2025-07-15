using RabbitMQ.Client;

namespace NotificationQueue.Services.Interfaces;

public interface IRabbitMqService{
    Task<IConnection> CreateConnectionAsync();
}