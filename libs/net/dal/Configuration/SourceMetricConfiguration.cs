namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SourceMetricConfiguration : BaseTypeConfiguration<SourceMetric, int>
{
    public override void Configure(EntityTypeBuilder<SourceMetric> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        base.Configure(builder);
    }
}
