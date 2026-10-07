namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AnalysisBackfillConfiguration : AuditColumnsConfiguration<AnalysisBackfill>
{
    public override void Configure(EntityTypeBuilder<AnalysisBackfill> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.TimeZone).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Error).HasColumnType("text");

        builder.HasIndex(m => new { m.Status, m.CreatedOn }, "IX_analysis_backfill_status");

        base.Configure(builder);
    }
}
