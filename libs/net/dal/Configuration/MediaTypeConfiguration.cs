namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class MediaTypeConfiguration : BaseTypeConfiguration<MediaType, int>
{
    public override void Configure(EntityTypeBuilder<MediaType> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AutoTranscribe).IsRequired();
        builder.Property(m => m.DisableTranscribe).IsRequired();

        base.Configure(builder);
    }
}
