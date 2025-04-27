using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class RegisterUser {
    [Required(ErrorMessage = "Firstname is required")]
    public string Firstname { get; set; } = string.Empty;
    [Required(ErrorMessage = "Lastname is required")]
    public string Lastname { get; set; } = string.Empty;
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Date of Birth is required")]
    public DateTimeOffset DateOfBirthday { get; set; } 
    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;
    [Required(ErrorMessage = "Country is required")]
    public string Country { get; set; } = string.Empty;
    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = string.Empty;
}