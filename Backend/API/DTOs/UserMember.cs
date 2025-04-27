namespace API.DTOs;

public class UserMember{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public required string Username { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public string? ProfilePhoto { get; set; }
}