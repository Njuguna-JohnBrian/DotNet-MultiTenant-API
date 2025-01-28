using Microsoft.EntityFrameworkCore;
using MultitenancyApp.DatabaseContext.Models.ModelConfigs;

namespace MultitenancyApp.DatabaseContext;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TenantInfo> TenantInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TenantInfoConfig());
    }
}