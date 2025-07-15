namespace MeetApp.DataEntities.Entities.ManyToMany;

public class PostInterest{
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public int InterestId { get; set; }
    public Interest Interest { get; set; } = null!;
}