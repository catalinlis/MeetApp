namespace MeetApp.DataEntities.DTOs;

public class SqsQueueMessage
{
    public int Id { get; set; }
    public string BucketKey { get; set; } = null!;
    public string Type { get; set; } = null!;

}