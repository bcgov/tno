namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserAVOverviewConfiguration : AuditColumnsConfiguration<UserAVOverview>
{
    public override void Configure(EntityTypeBuilder<UserAVOverview> builder)
    {
        builder.HasKey(m => new { m.UserId, m.TemplateType });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.TemplateType).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.IsSubscribed).IsRequired();
        builder.Property(m => m.SendTo).IsRequired();

        builder.HasOne(m => m.User).WithMany(m => m.AVOverviewSubscriptionsManyToMany).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Template).WithMany(m => m.SubscribersManyToMany).HasForeignKey(m => m.TemplateType).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
