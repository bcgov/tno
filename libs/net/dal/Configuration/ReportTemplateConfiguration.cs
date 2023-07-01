namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportTemplateConfiguration : BaseTypeConfiguration<ReportTemplate, int>
{
    public override void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Subject).IsRequired().HasColumnType("text");
        builder.Property(m => m.Body).IsRequired().HasColumnType("text");
        builder.Property(m => m.EnableSections).IsRequired();
        builder.Property(m => m.EnableSectionSummary).IsRequired();
        builder.Property(m => m.EnableSummary).IsRequired();
        builder.Property(m => m.EnableCharts).IsRequired();
        builder.Property(m => m.EnableChartsOverTime).IsRequired();

        builder.HasMany(m => m.ChartTemplates).WithMany(m => m.ReportTemplates).UsingEntity<ReportTemplateChartTemplate>();

        builder.HasIndex(m => m.Name).IsUnique();

        base.Configure(builder);
    }
}
