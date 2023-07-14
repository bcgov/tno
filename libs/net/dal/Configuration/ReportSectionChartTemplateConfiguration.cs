namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportSectionChartTemplateConfiguration : AuditColumnsConfiguration<ReportSectionChartTemplate>
{
    public override void Configure(EntityTypeBuilder<ReportSectionChartTemplate> builder)
    {
        builder.HasKey(m => new { m.ReportSectionId, m.ChartTemplateId });
        builder.Property(m => m.ReportSectionId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ChartTemplateId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SortOrder).IsRequired();
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.ReportSection).WithMany(m => m.ChartTemplatesManyToMany).HasForeignKey(m => m.ReportSectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.ChartTemplate).WithMany(m => m.ReportSectionsManyToMany).HasForeignKey(m => m.ChartTemplateId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
