using Microsoft.EntityFrameworkCore;
using MultitenancyApp.DatabaseContext.Models;
using MultitenancyApp.DatabaseContext.Models.ModelConfigs;

namespace MultitenancyApp.DatabaseContext;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TenantConfig());
    }
}