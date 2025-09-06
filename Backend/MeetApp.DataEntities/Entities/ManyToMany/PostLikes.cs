namespace MeetApp.DataEntities.Entities.ManyToMany;

public class PostLikes{
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public DateTimeOffset LikedAt { get; set; }
}