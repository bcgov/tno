namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class NotificationTemplateConfiguration : BaseTypeConfiguration<NotificationTemplate, int>
{
    public override void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Subject).IsRequired().HasColumnType("text");
        builder.Property(m => m.Body).IsRequired().HasColumnType("text");
        builder.Property(m => m.IsPublic).IsRequired();
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(m => m.Name).IsUnique();
        builder.HasIndex(m => new { m.IsPublic, m.IsEnabled });

        base.Configure(builder);
    }
}
