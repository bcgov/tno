namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserAVOverviewInstanceConfiguration : AuditColumnsConfiguration<UserAVOverviewInstance>
{
    public override void Configure(EntityTypeBuilder<UserAVOverviewInstance> builder)
    {
        builder.HasKey(m => new { m.UserId, m.InstanceId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.InstanceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SentOn);
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.Response).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Instance).WithMany(m => m.UserInstances).HasForeignKey(m => m.InstanceId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
