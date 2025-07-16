using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NotificationQueue.Services.Interfaces;
using RabbitMQ.Client;
using MeetApp.DataEntities.Configurations;


namespace NotificationQueue.Services
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly RabbitMqConfiguration _configuration;
        private readonly ILogger<RabbitMqService> _logger;

        public RabbitMqService(IOptions<RabbitMqConfiguration> configuration, ILogger<RabbitMqService> logger)
        {
            _configuration = configuration.Value;
            _logger = logger;
        }

        public async Task<IConnection> CreateConnectionAsync()
        {
            int maxRetries = 10;
            int delayMs = 3000;
            int attempt = 0;

            var factory = new ConnectionFactory
            {
                HostName = _configuration.HostName,
                UserName = _configuration.Username,
                Password = _configuration.Password
            };

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;
                    var connection = await factory.CreateConnectionAsync();
                    _logger.LogInformation($"Connected to RabbitMQ on attempt {attempt}.");
                    return connection;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, $"Attempt {attempt} failed to connect to RabbitMQ.");

                    if (attempt >= maxRetries)
                    {
                        _logger.LogError("Max retry attempts reached. Throwing exception.");
                        throw;
                    }

                    await Task.Delay(delayMs);
                }
            }

            throw new Exception("Failed to connect to RabbitMQ after retries.");
        }
    }
}