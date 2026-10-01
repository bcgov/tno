namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentTopicConfiguration : AuditColumnsConfiguration<ContentTopic>
{
    public override void Configure(EntityTypeBuilder<ContentTopic> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.TopicId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.TopicId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Score).IsRequired();
        builder.Property(m => m.IsScoreOverridden).IsRequired().HasDefaultValue(false);
        builder.ToTable(t => t.HasCheckConstraint("CK_content_topic_score", "\"score\" >= 0"));

        builder.HasOne(m => m.Content).WithMany(m => m.TopicsManyToMany).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Topic).WithMany(m => m.ContentsManyToMany).HasForeignKey(m => m.TopicId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.ScoreRule).WithMany().HasForeignKey(m => m.ScoreRuleId).OnDelete(DeleteBehavior.SetNull);

        base.Configure(builder);
    }
}
