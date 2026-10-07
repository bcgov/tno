namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentFieldOwnershipConfiguration : AuditColumnsConfiguration<ContentFieldOwnership>
{
    public override void Configure(EntityTypeBuilder<ContentFieldOwnership> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.Field, m.ValueKey });
        builder.Property(m => m.Field).IsRequired().HasMaxLength(50);
        builder.Property(m => m.ValueKey).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Owner).IsRequired();

        builder.HasOne(m => m.Content).WithMany().HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ContentAnalysis>().WithMany().HasForeignKey(m => m.AnalysisId).OnDelete(DeleteBehavior.SetNull);

        base.Configure(builder);
    }
}
