namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AutomationStepConfiguration : BaseTypeConfiguration<AutomationStep, int>
{
    public override void Configure(EntityTypeBuilder<AutomationStep> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AutomationProfileId).IsRequired();
        builder.Property(m => m.Prompt).IsRequired().HasColumnType("text").HasDefaultValueSql("''");
        builder.Property(m => m.Target).IsRequired().HasMaxLength(20).HasDefaultValueSql("'content'");
        builder.Property(m => m.FilterId);
        builder.Property(m => m.ApplyToAutomationFilter).IsRequired().HasDefaultValue(false);
        builder.Property(m => m.IterateStepFilter).IsRequired().HasDefaultValue(false);
        builder.Property(m => m.LLMId);
        builder.Property(m => m.SendSeparatePrompts).IsRequired().HasDefaultValue(false);
        builder.Property(m => m.UseChatCompletions).IsRequired().HasDefaultValue(false);

        builder.HasOne(m => m.AutomationProfile).WithMany(m => m.Steps).HasForeignKey(m => m.AutomationProfileId).OnDelete(DeleteBehavior.Cascade);
        // Optional gate/enrichment filter; removing a filter should not delete the step.
        builder.HasOne(m => m.Filter).WithMany().HasForeignKey(m => m.FilterId).OnDelete(DeleteBehavior.SetNull);
        // Optional reference; removing an LLM should not delete the step (it falls back to the profile LLM).
        builder.HasOne(m => m.LLM).WithMany().HasForeignKey(m => m.LLMId).OnDelete(DeleteBehavior.SetNull);

        base.Configure(builder);
    }
}
