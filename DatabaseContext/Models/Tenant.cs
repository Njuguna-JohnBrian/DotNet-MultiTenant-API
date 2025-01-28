namespace MultitenancyApp.DatabaseContext.Models;

public class Tenant
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string TenantName { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}