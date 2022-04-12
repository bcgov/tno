namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SeriesConfiguration : BaseTypeConfiguration<Series, int>
{
    public override void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        base.Configure(builder);
    }
}
