namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class TopicScoreRuleConfiguration : AuditColumnsConfiguration<TopicScoreRule>
{
    public override void Configure(EntityTypeBuilder<TopicScoreRule> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.SourceId).IsRequired();
        builder.Property(m => m.SeriesId);
        builder.Property(m => m.Section).HasMaxLength(100);
        builder.Property(m => m.PageMin);
        builder.Property(m => m.PageMax);
        builder.Property(m => m.HasImage);
        builder.Property(m => m.CharacterMin);
        builder.Property(m => m.CharacterMax);
        builder.Property(m => m.TimeMin);
        builder.Property(m => m.TimeMax);
        builder.Property(m => m.Score).IsRequired();
        builder.Property(m => m.SortOrder).IsRequired();

        builder.HasOne(m => m.Source).WithMany(m => m.ScoreRules).HasForeignKey(m => m.SourceId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
        builder.HasOne(m => m.Series).WithMany(m => m.ScoreRules).HasForeignKey(m => m.SeriesId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.SourceId, m.SeriesId, m.Section }, "IX_source_id_series_id_section");

        base.Configure(builder);
    }
}
