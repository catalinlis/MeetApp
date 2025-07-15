namespace MeetApp.DataEntities.Entities;

public class PhotoLikes{
    public int PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public DateTimeOffset LikedAt { get; set; }
}