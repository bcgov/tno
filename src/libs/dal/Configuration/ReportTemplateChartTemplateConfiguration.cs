namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportTemplateChartTemplateConfiguration : AuditColumnsConfiguration<ReportTemplateChartTemplate>
{
    public override void Configure(EntityTypeBuilder<ReportTemplateChartTemplate> builder)
    {
        builder.HasKey(m => new { m.ReportTemplateId, m.ChartTemplateId });
        builder.Property(m => m.ReportTemplateId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ChartTemplateId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.ReportTemplate).WithMany(m => m.ChartTemplatesManyToMany).HasForeignKey(m => m.ReportTemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.ChartTemplate).WithMany(m => m.ReportTemplatesManyToMany).HasForeignKey(m => m.ChartTemplateId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
