namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class DataSourceMetricConfiguration : AuditColumnsConfiguration<DataSourceMetric>
{
    public override void Configure(EntityTypeBuilder<DataSourceMetric> builder)
    {
        builder.HasKey(m => new { m.DataSourceId, m.SourceMetricId });
        builder.Property(m => m.DataSourceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SourceMetricId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Earned).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.Reach).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.Impression).IsRequired().HasDefaultValue(0);

        builder.HasOne(m => m.DataSource).WithMany(m => m.MetricsManyToMany).HasForeignKey(m => m.DataSourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.SourceMetric).WithMany(m => m.DataSourcesManyToMany).HasForeignKey(m => m.SourceMetricId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
