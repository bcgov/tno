namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class IngestTypeConfiguration : BaseTypeConfiguration<IngestType, int>
{
    public override void Configure(EntityTypeBuilder<IngestType> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AutoTranscribe).IsRequired();
        builder.Property(m => m.DisableTranscribe).IsRequired();

        base.Configure(builder);
    }
}
