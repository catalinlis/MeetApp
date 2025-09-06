namespace MeetApp.DataEntities.Entities;

public class PostComment{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string Text { get; set; } = string.Empty;
    public AppUser CreatedByUser { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Post Post { get; set; } = null!;
}