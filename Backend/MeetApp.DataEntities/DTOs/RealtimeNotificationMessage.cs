namespace MeetApp.DataEntities.DTOs;

public class RealtimeNotificationMessage{
    public int Id { get; set; }
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string SenderFirstname { get; set; } = null!;
    public string SenderLastname { get; set; } = null!;
    public string SenderProfilePhoto { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}