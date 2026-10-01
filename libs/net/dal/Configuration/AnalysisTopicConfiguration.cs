namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AnalysisTopicConfiguration : AuditColumnsConfiguration<AnalysisTopic>
{
    public override void Configure(EntityTypeBuilder<AnalysisTopic> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Label).IsRequired().HasMaxLength(250);
        builder.Property(m => m.Key).IsRequired().HasMaxLength(250);
        builder.Property(m => m.Aliases).IsRequired();

        builder.HasOne(m => m.Topic).WithMany().HasForeignKey(m => m.TopicId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.Key, "IX_analysis_topic_key").IsUnique();

        base.Configure(builder);
    }
}
