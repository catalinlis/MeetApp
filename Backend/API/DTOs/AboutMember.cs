
namespace API.DTOs;

public class AboutMember{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public required DateTimeOffset DateOfBirth { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string Email { get; set; }
    public required string Gender { get; set; }
}