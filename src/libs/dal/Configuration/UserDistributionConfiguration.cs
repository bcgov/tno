namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserDistributionConfiguration : AuditColumnsConfiguration<UserDistribution>
{
    public override void Configure(EntityTypeBuilder<UserDistribution> builder)
    {
        builder.HasKey(m => new { m.UserId, m.LinkedUserId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.LinkedUserId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.User).WithMany(m => m.Distribution).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.LinkedUser).WithMany().HasForeignKey(m => m.LinkedUserId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
