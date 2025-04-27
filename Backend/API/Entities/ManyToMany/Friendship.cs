using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities.ManyToMany;

public class Friendship{
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public int FriendId { get; set; }
    public AppUser Friend { get; set; } = null!;
    public DateTimeOffset FriendsSince { get; set; }
}