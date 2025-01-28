using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MultitenancyApp.DatabaseContext.Models.ModelConfigs;

public class TenantConfig : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(x => x.Id);
        builder.HasIndex(t => t.Email).IsUnique();
        builder.HasIndex(t => t.TenantName).IsUnique();
    }
}