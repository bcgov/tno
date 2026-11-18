namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportSectionConfiguration : AuditColumnsConfiguration<ReportSection>
{
    public override void Configure(EntityTypeBuilder<ReportSection> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Property(m => m.IsEnabled);
        builder.Property(m => m.Description).IsRequired().HasColumnType("text").HasDefaultValueSql("''");
        builder.Property(m => m.SortOrder).HasDefaultValue(0);
        builder.Property(m => m.ReportId).IsRequired();
        builder.Property(m => m.SectionType).IsRequired();
        builder.Property(m => m.FilterId);
        builder.Property(m => m.FolderId);
        builder.Property(m => m.LinkedReportId);
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.Report).WithMany(m => m.Sections).HasForeignKey(m => m.ReportId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Filter).WithMany(m => m.ReportSections).HasForeignKey(m => m.FilterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Folder).WithMany(m => m.ReportSections).HasForeignKey(m => m.FolderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.LinkedReport).WithMany().HasForeignKey(m => m.LinkedReportId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.ChartTemplates).WithMany(m => m.ReportSections).UsingEntity<ReportSectionChartTemplate>();

        builder.HasIndex(m => new { m.IsEnabled, m.Name }, $"IX_{typeof(ReportSection).Name.ToLower()}_is_enabled");
        builder.HasIndex(m => new { m.ReportId, m.Name }).IsUnique();

        base.Configure(builder);
    }
}
