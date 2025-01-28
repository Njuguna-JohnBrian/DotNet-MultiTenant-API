using System.ComponentModel.DataAnnotations;

namespace MultitenancyApp.Requests;

public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public required string Email { get; set; } = string.Empty;

    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(5, ErrorMessage = "Password must be at least 5 characters")]
    public required string Password { get; set; } = string.Empty;
}