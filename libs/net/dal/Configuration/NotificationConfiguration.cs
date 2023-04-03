namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class NotificationConfiguration : BaseTypeConfiguration<Notification, int>
{
    public override void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.NotificationType).IsRequired();
        builder.Property(m => m.RequireAlert).IsRequired();
        builder.Property(m => m.Filter).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(m => m.Template).IsRequired().HasColumnType("text");
        builder.Property(m => m.Resend).IsRequired();
        builder.Property(m => m.OwnerId).IsRequired();
        builder.Property(m => m.IsPublic).IsRequired();

        builder.HasOne(m => m.Owner).WithMany(m => m.Notifications).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.Subscribers).WithMany(m => m.NotificationSubscriptions).UsingEntity<UserNotification>();

        builder.HasIndex(m => new { m.OwnerId, m.Name }).IsUnique();

        base.Configure(builder);
    }
}
