namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class IngestScheduleConfiguration : AuditColumnsConfiguration<IngestSchedule>
{
    public override void Configure(EntityTypeBuilder<IngestSchedule> builder)
    {
        builder.HasKey(m => new { m.IngestId, m.ScheduleId });
        builder.Property(m => m.IngestId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ScheduleId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.Ingest).WithMany(m => m.SchedulesManyToMany).HasForeignKey(m => m.IngestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Schedule).WithMany(m => m.IngestsManyToMany).HasForeignKey(m => m.ScheduleId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
