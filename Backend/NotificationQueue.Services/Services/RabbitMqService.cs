using Microsoft.Extensions.Options;
using NotificationQueue.Services.Interfaces;
using RabbitMQ.Client;
using MeetApp.DataEntities.Configurations;

namespace NotificationQueue.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly RabbitMqConfiguration _configuration;

    public RabbitMqService(IOptions<RabbitMqConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    public async Task<RabbitMQ.Client.IConnection> CreateConnectionAsync()
    {
        ConnectionFactory connection = new ConnectionFactory
        {
            HostName = _configuration.HostName,
            UserName = _configuration.Username,
            Password = _configuration.Password
        };

        var channel = await connection.CreateConnectionAsync();

        return channel;
    }
}