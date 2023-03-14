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

        builder.HasOne(m => m.Content).WithMany(m => m.TopicsManyToMany).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Topic).WithMany(m => m.ContentsManyToMany).HasForeignKey(m => m.TopicId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
