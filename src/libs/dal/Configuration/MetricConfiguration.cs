namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class MetricConfiguration : BaseTypeConfiguration<Metric, int>
{
    public override void Configure(EntityTypeBuilder<Metric> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        builder.HasIndex(m => m.Name, "IX_name").IsUnique();

        base.Configure(builder);
    }
}
