namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class NotificationInstanceConfiguration : AuditColumnsConfiguration<NotificationInstance>
{
    public override void Configure(EntityTypeBuilder<NotificationInstance> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.NotificationId).IsRequired();
        builder.Property(m => m.ContentId).IsRequired();
        builder.Property(m => m.OwnerId);
        builder.Property(m => m.SentOn);
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.Response).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(m => m.Subject).IsRequired().HasColumnType("text");
        builder.Property(m => m.Body).IsRequired().HasColumnType("text");

        builder.HasOne(m => m.Notification).WithMany(m => m.Instances).HasForeignKey(m => m.NotificationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Content).WithMany(m => m.NotificationsManyToMany).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.Status, m.SentOn });

        base.Configure(builder);
    }
}
