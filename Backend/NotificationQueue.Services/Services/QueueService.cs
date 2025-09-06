using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationQueue.Services.Interfaces;
using NotificationQueue.Helpers;

namespace NotificationQueue.Services;

public class QueueService : IQueueService
{
    private IChannel _channel;
    private IConnection _connection;
    private readonly IRabbitMqService _rabbitMqService;

    public QueueService(IRabbitMqService rabbitMqService)
    {
        _rabbitMqService = rabbitMqService;
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

                await Task.CompletedTask;
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

    public async Task WriteMessage(NotificationMessageQueue message){
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        Console.WriteLine(body);
        var props = new BasicProperties();
        props.ContentType = "application/json";

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: "notifications_queue",
            mandatory: true,
            basicProperties: props,
            body: body
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