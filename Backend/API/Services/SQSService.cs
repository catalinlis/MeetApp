using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using API.Services.Interfaces;
using MeetApp.DataEntities.Common;
using MeetApp.DataEntities.DTOs;

namespace API.Services;

public class SQSService : ISQSService
{
    private readonly IAmazonSQS _sqsclient;
    private readonly string _queueUrl;
    public SQSService(IAmazonSQS sqsClient, string queueUrl)
    {
        _sqsclient = sqsClient;
        _queueUrl = queueUrl;
    }

    public async Task<Result> SendQueueMessage(int id, string bucketKey, string type)
    {
        var message = new SqsQueueMessage
        {
            Id = id,
            BucketKey = bucketKey,
            Type = type
        };

        string messageBody = JsonSerializer.Serialize(message);

        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = messageBody
        };

        try
        {
            var response = await _sqsclient.SendMessageAsync(request);
            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result.Failure($"Message sent with ID: {ex.Message}");
        }



    }
    
}