namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class PrintContentConfiguration : AuditColumnsConfiguration<PrintContent>
{
    public override void Configure(EntityTypeBuilder<PrintContent> builder)
    {
        builder.HasKey(m => m.ContentId);
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Edition).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Section).IsRequired().HasMaxLength(100);
        builder.Property(m => m.StoryType).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Byline).IsRequired().HasMaxLength(500);

        builder.HasOne(m => m.Content).WithOne(m => m.PrintContent).HasForeignKey<PrintContent>(m => m.ContentId);

        builder.HasIndex(m => new { m.Edition, m.Section, m.StoryType });

        base.Configure(builder);
    }
}
