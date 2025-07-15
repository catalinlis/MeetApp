namespace MeetApp.DataEntities.DTOs;

public class Comment{
    public string Username { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTimeOffset AddedAt { get; set; }
}