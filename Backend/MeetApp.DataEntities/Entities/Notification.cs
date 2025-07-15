namespace MeetApp.DataEntities.Entities;

public class Notification{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int SenderUserId { get; set; }
    public AppUser SenderUser { get; set; } = null!;
    public int TargetUserId { get; set; }
    public AppUser TargetUser { get; set; } = null!;
    public int? ResourceId { get; set; }
    public string? ResourceType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public bool Seen { get; set; } = false;
    public bool Opened { get; set; } = false;

}