
namespace API.Entities.ManyToMany;

public class UserInterest{
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public int InterestId { get; set; }
    public Interest Interest { get; set; } = null!;
}