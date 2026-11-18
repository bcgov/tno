namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentAnalysisConfiguration : AuditColumnsConfiguration<ContentAnalysis>
{
    public override void Configure(EntityTypeBuilder<ContentAnalysis> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.InputHash).IsRequired().HasMaxLength(64);
        builder.Property(m => m.NormalizationVersion).IsRequired().HasMaxLength(20);
        builder.Property(m => m.SchemaVersion).IsRequired().HasMaxLength(20);
        builder.Property(m => m.PromptVersion).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Model).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Summary).IsRequired().HasColumnType("text");
        builder.Property(m => m.PrimaryTopic).HasMaxLength(250);
        builder.Property(m => m.SuggestedContributor).HasMaxLength(250);
        builder.Property(m => m.KeyFacts).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.Entities).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.Places).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.Topics).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.SuggestedTags).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.Events).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.Quotes).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.Validation).IsRequired().HasColumnType("jsonb");

        builder.HasOne(m => m.Content).WithMany().HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.AnalysisTopic).WithMany().HasForeignKey(m => m.AnalysisTopicId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<LLM>().WithMany().HasForeignKey(m => m.LLMId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => new { m.ContentId, m.InputHash }, "IX_content_analysis_input").IsUnique();
        builder.HasIndex(m => new { m.ContentId, m.IsCurrent }, "IX_content_analysis_current");

        base.Configure(builder);
    }
}
