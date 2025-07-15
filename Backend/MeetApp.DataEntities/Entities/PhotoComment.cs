namespace MeetApp.DataEntities.Entities;

public class PhotoComment
{
    public int Id { get; set; }
    public int PhotoId { get; set; }
    public string Text { get; set; } = string.Empty;
    public AppUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Photo Photo { get; set; } = null!;
}