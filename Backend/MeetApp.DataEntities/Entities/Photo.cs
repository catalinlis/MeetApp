namespace MeetApp.DataEntities.Entities;

public class Photo{
    public int Id { get; set; }
    public string PhotoId { get; set; } = string.Empty;
    public bool Resized { get; set; } = false;
    public string? ResizedPhotoId { get; set; }
    public string? Text { get; set; }
    public int AddedById { get; set; }
    public AppUser AddedBy { get; set; } = null!;
    public int LikesCount { get; set; } = 0;
    public List<PhotoLikes> LikedByUsers { get; set; } = new List<PhotoLikes>();
    public List<AppUser> TaggedUsers { get; set; } = new List<AppUser>();
    public int CommentsCount { get; set; } = 0;
    public List<PhotoComment> Comments { get; set; } = new List<PhotoComment>();
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}