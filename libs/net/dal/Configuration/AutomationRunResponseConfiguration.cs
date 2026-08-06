namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AutomationRunResponseConfiguration : AuditColumnsConfiguration<AutomationRunResponse>
{
    public override void Configure(EntityTypeBuilder<AutomationRunResponse> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AutomationRunId).IsRequired();
        builder.Property(m => m.StepId).IsRequired();
        builder.Property(m => m.StepName).IsRequired().HasMaxLength(100).HasDefaultValueSql("''");
        builder.Property(m => m.ActionName).HasMaxLength(100);
        builder.Property(m => m.ContentId);
        builder.Property(m => m.Prompt).HasColumnType("text");
        builder.Property(m => m.Response).IsRequired().HasColumnType("text").HasDefaultValueSql("''");

        builder.HasOne(m => m.AutomationRun).WithMany(m => m.Responses).HasForeignKey(m => m.AutomationRunId).OnDelete(DeleteBehavior.Cascade);

        // Supports fetching a run's responses (run detail/diff) ordered as captured.
        builder.HasIndex(m => m.AutomationRunId, "IX_automation_run_response_run");

        base.Configure(builder);
    }
}
