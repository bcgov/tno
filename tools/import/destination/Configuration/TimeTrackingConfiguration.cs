namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class TimeTrackingConfiguration : IEntityTypeConfiguration<TimeTracking>
{
    public virtual void Configure(EntityTypeBuilder<TimeTracking> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.UserId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Effort).IsRequired();
        builder.Property(m => m.Activity).IsRequired();

        builder.HasOne(m => m.Content).WithMany(m => m.TimeTrackings).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.User).WithMany(m => m.TimeTrackings).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}