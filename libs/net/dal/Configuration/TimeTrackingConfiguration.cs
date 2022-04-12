namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class TimeTrackingConfiguration : AuditColumnsConfiguration<TimeTracking>
{
    public override void Configure(EntityTypeBuilder<TimeTracking> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Effort).IsRequired();
        builder.Property(m => m.Activity).IsRequired().HasMaxLength(100);

        builder.HasOne(m => m.Content).WithMany(m => m.TimeTrackings).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.User).WithMany(m => m.TimeTrackings).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
