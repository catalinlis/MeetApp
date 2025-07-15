using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using MeetApp.DataEntities.Entities.ManyToMany;

namespace MeetApp.DataEntities.Entities;

public class AppUser : IdentityUser<int> {
    [Required(ErrorMessage = "Firstname is required")]
    public string Firstname { get; set; } = string.Empty;
    [Required(ErrorMessage = "Lastname is required")]
    public string Lastname { get; set; } = string.Empty;
    [Required(ErrorMessage = "Email is required")]
    public override string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;
    [Required(ErrorMessage = "Date of Birth required")]
    public DateTimeOffset DateOfBirth { get; set; }
    [Required(ErrorMessage = "Country is required")]
    public string Country { get; set; } = string.Empty;
    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = string.Empty;
    public string ProfilePhoto { get; set; } = string.Empty;
    public List<Photo> Photos { get; set; } = new List<Photo>();
    public List<PhotoLikes> PhotosLiked { get; set; } = new List<PhotoLikes>();
    public List<Post> Posts { get; set; } = new List<Post>();
    public List<Interest> Interests { get; set; } = new List<Interest>();
    public List<PostLikes> PostLiked { get; set; } = new List<PostLikes>();
    [JsonIgnore]
    public List<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
    [JsonIgnore]
    public List<FriendRequest> SentFriendRequests { get; set; } = new List<FriendRequest>();
    [JsonIgnore]
    public List<FriendRequest> ReceivedFriendRequest { get; set; } = new List<FriendRequest>();
    [JsonIgnore]
    public ICollection<Friendship> Friends { get; set; } = null!;
    public ICollection<Chat> Chats { get; set; } = null!;
    public ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
    public ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
    public int RegisterStep { get; set; } 
}