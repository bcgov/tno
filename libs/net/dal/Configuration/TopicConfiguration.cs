namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class TopicConfiguration : BaseTypeConfiguration<Topic, int>
{
    public override void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.TopicType).IsRequired();
        builder.Property(m => m.IsSystem).IsRequired().HasDefaultValue(false);

        builder.HasIndex(m => m.Name, "IX_topic_name").IsUnique();

        base.Configure(builder);
    }
}
