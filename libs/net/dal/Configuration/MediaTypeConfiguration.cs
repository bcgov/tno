namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class MediaTypeConfiguration : BaseTypeConfiguration<MediaType, int>
{
    public override void Configure(EntityTypeBuilder<MediaType> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        base.Configure(builder);
    }
}
