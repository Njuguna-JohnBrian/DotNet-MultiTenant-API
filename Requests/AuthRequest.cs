using System.ComponentModel.DataAnnotations;

namespace MultitenancyApp.Requests;

public class AuthRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public required string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "TenantName is required")]
    [MinLength(5, ErrorMessage = "TenantName must be at least 5 characters")]
    public required string TenantName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(5, ErrorMessage = "Password must be at least 5 characters")]
    public required string Password { get; set; } = string.Empty;
}