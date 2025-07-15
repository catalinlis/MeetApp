using NotificationQueue.Services.Interfaces;
using NotificationQueue.Helpers;
using Notification.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


namespace Notification.Services;

public class ConsumerQueueService : IConsumerQueueService{

    private IChannel _channel;
    private IConnection _connection;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly INotificationDispatcher _notificationDispatcher;

    public ConsumerQueueService(IRabbitMqService rabbitMqService, INotificationDispatcher notificationDispatcher)
    {
        _rabbitMqService = rabbitMqService;
        _notificationDispatcher = notificationDispatcher;
    }
    public async Task InitAsync()
    {
        _connection = await _rabbitMqService.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        await _channel.QueueDeclareAsync(
            queue: "notifications_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public async Task ReadMessage(){
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);

            try
            {
                var message = JsonSerializer.Deserialize<NotificationMessageQueue>(messageJson);
                ObjectPrinter.PrintProperties(message);

                Console.WriteLine("---------------------------------------------");

                await _notificationDispatcher.DispatchAsync(message);
                
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            { Console.WriteLine(ex); }
        };

        await _channel.BasicConsumeAsync(
            queue: "notifications_queue",
            autoAck: false,
            consumer: consumer
        );

        await Task.CompletedTask;
    }

    public async Task CloseAsync(){
        if(_channel.IsOpen)
            await _channel.CloseAsync();
        if(_connection.IsOpen)
            await _connection.CloseAsync();
    }
}