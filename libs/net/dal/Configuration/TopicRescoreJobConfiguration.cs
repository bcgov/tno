namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class TopicRescoreJobConfiguration : AuditColumnsConfiguration<TopicRescoreJob>
{
    public override void Configure(EntityTypeBuilder<TopicRescoreJob> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.StartOn).IsRequired();
        builder.Property(m => m.EndOn).IsRequired();
        builder.Property(m => m.SourceIds).IsRequired();
        builder.Property(m => m.Error).HasColumnType("text");

        builder.HasIndex(m => new { m.Status, m.CreatedOn }, "IX_topic_rescore_job_status");

        base.Configure(builder);
    }
}
