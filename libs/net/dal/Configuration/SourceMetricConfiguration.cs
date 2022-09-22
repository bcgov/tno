namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SourceMetricConfiguration : AuditColumnsConfiguration<SourceMetric>
{
    public override void Configure(EntityTypeBuilder<SourceMetric> builder)
    {
        builder.HasKey(m => new { m.SourceId, m.MetricId });
        builder.Property(m => m.SourceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.MetricId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Earned).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.Reach).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.Impression).IsRequired().HasDefaultValue(0);

        builder.HasOne(m => m.Source).WithMany(m => m.MetricsManyToMany).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Metric).WithMany(m => m.SourcesManyToMany).HasForeignKey(m => m.MetricId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
