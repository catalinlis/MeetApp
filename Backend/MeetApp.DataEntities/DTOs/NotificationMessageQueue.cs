public class NotificationMessageQueue{
    public string Type { get; set; } = string.Empty;
    public int SenderUserId { get; set; }
    public int TargetUserId { get; set; }
    public int? ResourceId { get; set; }
    public string? ResourceType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}