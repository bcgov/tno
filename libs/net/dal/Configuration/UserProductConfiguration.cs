namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserProductConfiguration : AuditColumnsConfiguration<UserProduct>
{
    public override void Configure(EntityTypeBuilder<UserProduct> builder)
    {
        builder.HasKey(m => new { m.UserId, m.ProductId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ProductId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.IsSubscribed).IsRequired();
        builder.Property(m => m.RequestedIsSubscribedStatus);
        builder.Property(m => m.SubscriptionChangeActioned);

        builder.HasOne(m => m.User).WithMany(m => m.ProductSubscriptionsManyToMany).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
