namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class DataLocationConfiguration : BaseTypeConfiguration<DataLocation, int>
{
    public override void Configure(EntityTypeBuilder<DataLocation> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        base.Configure(builder);
    }
}
