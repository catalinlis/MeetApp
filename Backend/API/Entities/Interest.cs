using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using API.Entities.ManyToMany;

namespace API.Entities;

public class Interest{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
     public int Id { get; set; }
    [Required(ErrorMessage = "Interest key is required")]
    public string InterestKey { get; set; } = string.Empty;
    [Required(ErrorMessage = "Interest name is required")]
    public string InterestName { get; set; } = string.Empty;
    [JsonIgnore]
    public List<AppUser> Users { get; set; } = new List<AppUser>();
    [JsonIgnore]
    public List<UserInterest> UsersInterest { get; set; } =  null!;

}