using MultitenancyApp.Helpers;
using MultitenancyApp.Interfaces;

namespace MultitenancyApp.DatabaseContext;

using Microsoft.EntityFrameworkCore;

public class DbSeeder
{
    private readonly IdentityDbContext _identityContext;
    private readonly ApplicationDbContext _applicationContext;
    private IPasswordService _passwordService;

    public DbSeeder(IdentityDbContext identityContext, ApplicationDbContext applicationContext, IPasswordService passwordService)
    {
        _identityContext = identityContext;
        _applicationContext = applicationContext;
        _passwordService = passwordService;
    }


    public async Task SeedAsync()
    {
        await _identityContext.Database.MigrateAsync();
        await _applicationContext.Database.MigrateAsync();
        
        var tenants = await _identityContext.Tenants.ToListAsync();

        foreach (var optionsBuilder in tenants.Select(tenant => new Npgsql.NpgsqlConnectionStringBuilder
                 {
                     Host = ConfigurationHelper.GetConfigurationValueByKey("DbOptions:Host"),
                     Port = int.Parse(ConfigurationHelper.GetConfigurationValueByKey("DbOptions:Port")!),
                     Database = tenant.TenantName,
                     Username = tenant.TenantName,
                     Password = _passwordService.DecryptPassword(tenant.PasswordHash)
                 }.ConnectionString).Select(tenantConnectionString => new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(tenantConnectionString)))
        {
            await using var context = new ApplicationDbContext(optionsBuilder.Options);
            await context.Database.MigrateAsync();
        }
    }
}