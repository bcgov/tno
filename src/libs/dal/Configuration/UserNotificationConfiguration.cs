namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserNotificationConfiguration : AuditColumnsConfiguration<UserNotification>
{
    public override void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.HasKey(m => new { m.UserId, m.NotificationId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.NotificationId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Resend);
        builder.Property(m => m.IsSubscribed).IsRequired();

        builder.HasOne(m => m.User).WithMany(m => m.NotificationSubscriptionsManyToMany).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Notification).WithMany(m => m.SubscribersManyToMany).HasForeignKey(m => m.NotificationId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
