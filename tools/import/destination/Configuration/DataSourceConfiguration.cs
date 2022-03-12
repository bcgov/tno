namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class DataSourceConfiguration : IEntityTypeConfiguration<DataSource>
{
    public virtual void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired();
        builder.Property(m => m.Code).IsRequired();
        builder.Property(m => m.DataLocationId).IsRequired();
        builder.Property(m => m.MediaTypeId).IsRequired();
        builder.Property(m => m.LicenseId).IsRequired();
        builder.Property(m => m.Topic).IsRequired();
        builder.Property(m => m.Connection).IsRequired();

        builder.HasOne(m => m.DataLocation).WithMany(m => m.DataSources).HasForeignKey(m => m.DataLocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.MediaType).WithMany(m => m.DataSources).HasForeignKey(m => m.MediaTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.License).WithMany(m => m.DataSources).HasForeignKey(m => m.LicenseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Parent).WithMany().HasForeignKey(m => m.ParentId).OnDelete(DeleteBehavior.ClientSetNull);
    }
}