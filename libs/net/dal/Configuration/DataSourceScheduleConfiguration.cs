namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class DataScheduleConfiguration : AuditColumnsConfiguration<DataSourceSchedule>
{
    public override void Configure(EntityTypeBuilder<DataSourceSchedule> builder)
    {
        builder.HasKey(m => new { m.DataSourceId, m.ScheduleId });
        builder.Property(m => m.DataSourceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ScheduleId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.DataSource).WithMany(m => m.SchedulesManyToMany).HasForeignKey(m => m.DataSourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Schedule).WithMany(m => m.DataSourcesManyToMany).HasForeignKey(m => m.ScheduleId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
