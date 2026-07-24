namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AutomationRunConfiguration : AuditColumnsConfiguration<AutomationRun>
{
    public override void Configure(EntityTypeBuilder<AutomationRun> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AutomationProfileId).IsRequired();
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.Trigger).IsRequired().HasMaxLength(50).HasDefaultValueSql("'manual'");
        builder.Property(m => m.Note).HasColumnType("text");
        builder.Property(m => m.StartedOn).IsRequired();
        builder.Property(m => m.CompletedOn);
        builder.Property(m => m.Summary).HasColumnType("text");

        builder.HasOne(m => m.AutomationProfile).WithMany(m => m.Runs).HasForeignKey(m => m.AutomationProfileId).OnDelete(DeleteBehavior.Cascade);

        // Supports history queries by profile and retention pruning by age.
        builder.HasIndex(m => new { m.AutomationProfileId, m.StartedOn }, "IX_automation_run_profile_started");
        builder.HasIndex(m => m.StartedOn, "IX_automation_run_started");

        base.Configure(builder);
    }
}
