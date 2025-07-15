
namespace MeetApp.DataEntities.DTOs;

public class FeedItem{
    public string Type { get; set; } = null!;
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Author { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public bool LikedByMe { get; set; }
    public int CommentsCount { get; set; }
    public int PostId { get; set; }
    public List<InterestDTO> Interests { get; set; } = new List<InterestDTO>();
}