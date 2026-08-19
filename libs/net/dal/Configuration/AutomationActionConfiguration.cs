namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AutomationActionConfiguration : AuditColumnsConfiguration<AutomationAction>
{
    public override void Configure(EntityTypeBuilder<AutomationAction> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100).HasDefaultValueSql("''");
        builder.Property(m => m.AutomationStepId).IsRequired();
        builder.Property(m => m.Prompt).IsRequired().HasColumnType("text").HasDefaultValueSql("''");
        builder.Property(m => m.ActionType).IsRequired().HasMaxLength(100);
        builder.Property(m => m.MaxCalls);
        builder.Property(m => m.ConfirmationStatement).IsRequired().HasColumnType("text").HasDefaultValueSql("''");
        builder.Property(m => m.ContentField).HasMaxLength(100);
        builder.Property(m => m.ContentActionId);
        builder.Property(m => m.ReportId);
        builder.Property(m => m.NotificationId);
        builder.Property(m => m.FilterId);
        builder.Property(m => m.PriorActionId);
        builder.Property(m => m.AutoExecute).IsRequired().HasDefaultValue(false);
        builder.Property(m => m.AbortIfNoConfirmation).IsRequired().HasDefaultValue(false);
        builder.Property(m => m.LLMId);
        builder.Property(m => m.Objective).HasMaxLength(100);
        builder.Property(m => m.WorksOn).HasMaxLength(100);
        builder.Property(m => m.CreateIdentifier).HasMaxLength(100);
        builder.Property(m => m.CreateClone).IsRequired().HasDefaultValue(false);
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'");
        builder.Property(m => m.IsEnabled);
        builder.Property(m => m.SortOrder).HasDefaultValue(0);

        builder.HasOne(m => m.AutomationStep).WithMany(m => m.Actions).HasForeignKey(m => m.AutomationStepId).OnDelete(DeleteBehavior.Cascade);
        // Optional reference; removing a content action should not delete the automation action.
        builder.HasOne(m => m.ContentAction).WithMany().HasForeignKey(m => m.ContentActionId).OnDelete(DeleteBehavior.SetNull);
        // Optional reference; removing a report should not delete the automation action.
        builder.HasOne(m => m.Report).WithMany().HasForeignKey(m => m.ReportId).OnDelete(DeleteBehavior.SetNull);
        // Optional reference; removing a notification should not delete the automation action.
        builder.HasOne(m => m.Notification).WithMany().HasForeignKey(m => m.NotificationId).OnDelete(DeleteBehavior.SetNull);
        // Optional reference; removing a filter should not delete the automation action.
        builder.HasOne(m => m.Filter).WithMany().HasForeignKey(m => m.FilterId).OnDelete(DeleteBehavior.SetNull);
        // Optional self-reference; removing the prior action should not delete the dedupe action.
        builder.HasOne(m => m.PriorAction).WithMany().HasForeignKey(m => m.PriorActionId).OnDelete(DeleteBehavior.SetNull);
        // Optional reference; removing an LLM should not delete the action (it falls back to the step/profile LLM).
        builder.HasOne(m => m.LLM).WithMany().HasForeignKey(m => m.LLMId).OnDelete(DeleteBehavior.SetNull);

        base.Configure(builder);
    }
}
