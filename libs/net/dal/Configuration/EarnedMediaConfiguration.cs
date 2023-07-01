namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class EarnedMediaConfiguration : AuditColumnsConfiguration<EarnedMedia>
{
    public override void Configure(EntityTypeBuilder<EarnedMedia> builder)
    {
        builder.HasKey(m => new { m.SourceId, m.ContentType });
        builder.Property(m => m.SourceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ContentType).IsRequired();
        builder.Property(m => m.LengthOfContent).IsRequired();
        builder.Property(m => m.Rate).IsRequired();

        builder.HasOne(m => m.Source).WithMany(m => m.EarnedMedia).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
