namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class LicenseConfiguration : BaseTypeConfiguration<License, int>
{
    public override void Configure(EntityTypeBuilder<License> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.TTL).IsRequired();

        base.Configure(builder);
    }
}
