namespace MultitenancyApp.Interfaces;

public interface ITenantService
{
    Task CreateTenantDatabaseAsync(string tenantName, string dbPassword);
}