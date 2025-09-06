namespace MeetApp.DataEntities.DTOs;

public class PostDTO{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ImageId { get; set; }
    public int CreatedById { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}