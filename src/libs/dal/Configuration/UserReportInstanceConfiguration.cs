namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserReportInstanceConfiguration : AuditColumnsConfiguration<UserReportInstance>
{
    public override void Configure(EntityTypeBuilder<UserReportInstance> builder)
    {
        builder.HasKey(m => new { m.UserId, m.InstanceId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.InstanceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.LinkSentOn);
        builder.Property(m => m.LinkStatus).IsRequired();
        builder.Property(m => m.LinkResponse).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(m => m.TextSentOn);
        builder.Property(m => m.TextStatus).IsRequired();
        builder.Property(m => m.TextResponse).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Instance).WithMany(m => m.UserInstances).HasForeignKey(m => m.InstanceId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.LinkSentOn, m.LinkStatus }, "IX_user_report_instance_link");
        builder.HasIndex(m => new { m.TextSentOn, m.TextStatus }, "IX_user_report_instance_text");

        base.Configure(builder);
    }
}
