namespace API.Entities.ManyToMany;

public class FriendRequest{

    public int SentUserId { get; set; }
    public AppUser SentUser { get; set; } = null!;
    public int ReceiverUserId { get; set; }
    public AppUser ReceivedUser { get; set; } = null!;
    public DateTimeOffset SentAt { get; set; }
    public bool Accepted { get; set; } = false;

}