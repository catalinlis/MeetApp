
using System.Text.Json.Serialization;
using MeetApp.DataEntities.Entities.ManyToMany;

namespace MeetApp.DataEntities.Entities;

public class Post{
    public int Id { get; set; }
    public string? ImageId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    [JsonIgnore]
    public AppUser CreatedBy { get; set; } = null!;
    public int CommentsCount { get; set; } = 0;
    public List<PostComment> Comments { get; set; } = new List<PostComment>();
    public int LikesCount { get; set; } = 0;
    public List<PostLikes> LikedByUsers { get; set; } = new List<PostLikes>();
    public List<PostInterest> PostInterests { get; set; } = new List<PostInterest>();
    public string? Visibility { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

}