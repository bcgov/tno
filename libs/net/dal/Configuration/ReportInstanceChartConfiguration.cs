namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportInstanceChartConfiguration : AuditColumnsConfiguration<ReportInstanceChart>
{
    public override void Configure(EntityTypeBuilder<ReportInstanceChart> builder)
    {
        builder.HasKey(m => new { m.InstanceId, m.SectionId, m.ChartTemplateId });
        builder.Property(m => m.InstanceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SectionId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ChartTemplateId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SectionName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Data).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.DocCount).IsRequired();
        builder.Property(m => m.IsTruncated).IsRequired();
        builder.Property(m => m.GeneratedOn).IsRequired();

        builder.HasOne(m => m.Instance).WithMany(m => m.Charts).HasForeignKey(m => m.InstanceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Section).WithMany().HasForeignKey(m => m.SectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.ChartTemplate).WithMany().HasForeignKey(m => m.ChartTemplateId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
