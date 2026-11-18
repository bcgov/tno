namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserContentNotificationConfiguration : AuditColumnsConfiguration<UserContentNotification>
{
    public override void Configure(EntityTypeBuilder<UserContentNotification> builder)
    {
        builder.HasKey(m => new { m.UserId, m.ContentId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.IsSubscribed).IsRequired();

        builder.HasOne(m => m.User).WithMany(m => m.ContentNotifications).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Content).WithMany(m => m.UserNotifications).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
