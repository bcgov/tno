namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportInstanceContentConfiguration : AuditColumnsConfiguration<ReportInstanceContent>
{
    public override void Configure(EntityTypeBuilder<ReportInstanceContent> builder)
    {
        builder.HasKey(m => new { m.InstanceId, m.ContentId, m.SectionName });
        builder.Property(m => m.InstanceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SectionName).HasMaxLength(100).HasDefaultValueSql("''");

        builder.HasOne(m => m.Instance).WithMany(m => m.ContentManyToMany).HasForeignKey(m => m.InstanceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Content).WithMany(m => m.ReportsManyToMany).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
