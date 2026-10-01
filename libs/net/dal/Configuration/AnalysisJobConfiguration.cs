namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AnalysisJobConfiguration : AuditColumnsConfiguration<AnalysisJob>
{
    public override void Configure(EntityTypeBuilder<AnalysisJob> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.InputHash).IsRequired().HasMaxLength(64);
        builder.Property(m => m.Reason).IsRequired();
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.ClaimedBy).HasMaxLength(250);
        builder.Property(m => m.LastError).HasColumnType("text");

        builder.HasOne(m => m.Content).WithMany().HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Backfill).WithMany().HasForeignKey(m => m.BackfillId).OnDelete(DeleteBehavior.SetNull);

        // One job per content item; claims take due work by priority, then due time.
        builder.HasIndex(m => m.ContentId, "IX_analysis_job_content_id").IsUnique();
        builder.HasIndex(m => new { m.Status, m.Priority, m.DueOn }, "IX_analysis_job_queue");
        builder.HasIndex(m => m.BackfillId, "IX_analysis_job_backfill_id");

        base.Configure(builder);
    }
}
