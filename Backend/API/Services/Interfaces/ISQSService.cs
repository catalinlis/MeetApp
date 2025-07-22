using MeetApp.DataEntities.Common;

namespace API.Services.Interfaces;

public interface ISQSService
{
    Task<Result> SendQueueMessage(int id, string bucketKey, string type);
}