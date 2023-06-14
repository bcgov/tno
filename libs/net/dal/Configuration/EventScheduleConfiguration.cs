namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class EventScheduleConfiguration : AuditColumnsConfiguration<EventSchedule>
{
    public override void Configure(EntityTypeBuilder<EventSchedule> builder)
    {
        builder.HasKey(m => new { m.Id });
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ScheduleId).IsRequired();
        builder.Property(m => m.EventType).IsRequired();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.IsEnabled).IsRequired();
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(m => m.LastRanOn);

        builder.HasOne(m => m.Schedule).WithMany(m => m.Events).HasForeignKey(m => m.ScheduleId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
