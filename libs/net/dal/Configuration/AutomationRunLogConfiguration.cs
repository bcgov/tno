namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AutomationRunLogConfiguration : AuditColumnsConfiguration<AutomationRunLog>
{
    public override void Configure(EntityTypeBuilder<AutomationRunLog> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AutomationRunId).IsRequired();
        builder.Property(m => m.StepName).IsRequired().HasMaxLength(100).HasDefaultValueSql("''");
        builder.Property(m => m.ActionName).HasMaxLength(100);
        builder.Property(m => m.ActionType).HasMaxLength(50);
        builder.Property(m => m.AnalysisName).HasMaxLength(100);
        builder.Property(m => m.ContentId);
        builder.Property(m => m.Attempt).IsRequired().HasDefaultValue(1);
        builder.Property(m => m.IsLLM).IsRequired().HasDefaultValue(false);
        builder.Property(m => m.Variant).HasMaxLength(1);
        builder.Property(m => m.Prompt).HasColumnType("text");
        builder.Property(m => m.Response).HasColumnType("text");
        builder.Property(m => m.PromptTokens);
        builder.Property(m => m.CompletionTokens);
        builder.Property(m => m.DurationMs).IsRequired().HasDefaultValue(0L);
        builder.Property(m => m.Outcome).IsRequired().HasMaxLength(30).HasDefaultValueSql("''");
        builder.Property(m => m.Detail).HasColumnType("text");

        builder.HasOne(m => m.AutomationRun).WithMany(m => m.Logs).HasForeignKey(m => m.AutomationRunId).OnDelete(DeleteBehavior.Cascade);

        // Supports the log viewer (by run, optionally narrowed to a content item) and the
        // same-day retention sweep (by created date).
        builder.HasIndex(m => m.AutomationRunId, "IX_automation_run_log_run");
        builder.HasIndex(m => m.ContentId, "IX_automation_run_log_content");
        builder.HasIndex(m => m.CreatedOn, "IX_automation_run_log_created");

        base.Configure(builder);
    }
}
