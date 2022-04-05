namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ScheduleConfiguration : AuditColumnsConfiguration<Schedule>
{
    public override void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().HasMaxLength(6).ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.IsEnabled);
        builder.Property(m => m.ScheduleType);
        builder.Property(m => m.DelayMS);
        builder.Property(m => m.RunOn);
        builder.Property(m => m.StartAt);
        builder.Property(m => m.StopAt);
        builder.Property(m => m.Repeat);
        builder.Property(m => m.RunOnWeekDays);
        builder.Property(m => m.RunOnMonths);
        builder.Property(m => m.DayOfMonth);

        base.Configure(builder);
    }
}
