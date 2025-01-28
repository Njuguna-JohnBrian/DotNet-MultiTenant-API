namespace MultitenancyApp.DatabaseContext;

using Microsoft.EntityFrameworkCore;

public class DbSeeder
{
    private readonly IdentityDbContext _identityContext;
    private readonly ApplicationDbContext _applicationContext;

    public DbSeeder(IdentityDbContext identityContext, ApplicationDbContext applicationContext)
    {
        _identityContext = identityContext;
        _applicationContext = applicationContext;
    }


    public async Task SeedAsync()
    {
        await _identityContext.Database.MigrateAsync();
        await _applicationContext.Database.MigrateAsync();
    }
}