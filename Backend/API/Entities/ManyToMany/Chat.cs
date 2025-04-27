namespace API.Entities;

public class Chat{
    public int ChatFirstUserId { get; set; }
    public AppUser ChatFristUser { get; set; } = null!;
    public int ChatSecondUserId { get; set; }
    public AppUser ChatSecondUser { get; set; } = null!;
    public DateTimeOffset LastMessageSentAt { get; set; }
}