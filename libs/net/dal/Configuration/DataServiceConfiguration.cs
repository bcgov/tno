namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class DataServiceConfiguration : IEntityTypeConfiguration<DataService>
{
    public void Configure(EntityTypeBuilder<DataService> builder)
    {
        builder.HasKey(m => m.DataSourceId);
        builder.Property(m => m.DataSourceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.LastRanOn);
        builder.Property(m => m.FailedAttempts).HasDefaultValue(0);

        builder.HasOne(m => m.DataSource).WithOne(m => m.DataService).HasForeignKey<DataService>(m => m.DataSourceId).OnDelete(DeleteBehavior.Cascade);
    }
}
