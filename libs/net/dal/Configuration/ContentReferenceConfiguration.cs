namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentReferenceConfiguration : AuditColumnsConfiguration<ContentReference>
{
    public override void Configure(EntityTypeBuilder<ContentReference> builder)
    {
        builder.HasKey(m => new { m.Source, m.Uid });
        builder.Property(m => m.Source).IsRequired().HasMaxLength(100).ValueGeneratedNever();
        builder.Property(m => m.Uid).IsRequired().HasMaxLength(500).ValueGeneratedNever();
        builder.Property(m => m.Topic).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Offset);
        builder.Property(m => m.Partition);
        builder.Property(m => m.PublishedOn);
        builder.Property(m => m.SourceUpdateOn);

        builder.HasIndex(m => new { m.PublishedOn, m.Partition, m.Offset, m.Status });

        base.Configure(builder);
    }
}
