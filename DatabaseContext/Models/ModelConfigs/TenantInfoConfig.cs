using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MultitenancyApp.DatabaseContext.Models.ModelConfigs;

public class TenantInfoConfig : IEntityTypeConfiguration<TenantInfo>
{
    public void Configure(EntityTypeBuilder<TenantInfo> builder)
    {
        builder.ToTable("TenantInfo");
        builder.HasKey(x => x.Id);
    }
}