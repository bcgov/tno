namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class DataLocationConfiguration : BaseTypeConfiguration<DataLocation, int>
{
    public override void Configure(EntityTypeBuilder<DataLocation> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.LocationType).HasDefaultValue(DataLocationType.LocalVolume);
        builder.Property(m => m.Connection).IsRequired().HasColumnType("json");

        base.Configure(builder);
    }
}
