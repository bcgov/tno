namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserReportConfiguration : AuditColumnsConfiguration<UserReport>
{
    public override void Configure(EntityTypeBuilder<UserReport> builder)
    {
        builder.HasKey(m => new { m.UserId, m.ReportId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ReportId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.IsSubscribed).IsRequired();
        builder.Property(m => m.Format).IsRequired();
        builder.Property(m => m.SendTo).IsRequired();

        builder.HasOne(m => m.User).WithMany(m => m.ReportSubscriptionsManyToMany).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Report).WithMany(m => m.SubscribersManyToMany).HasForeignKey(m => m.ReportId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
