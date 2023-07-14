namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportSectionConfiguration : BaseTypeConfiguration<ReportSection, int>
{
    public override void Configure(EntityTypeBuilder<ReportSection> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ReportId).IsRequired();
        builder.Property(m => m.FilterId);
        builder.Property(m => m.FolderId);
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.Report).WithMany(m => m.Sections).HasForeignKey(m => m.ReportId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Filter).WithMany(m => m.ReportSections).HasForeignKey(m => m.FilterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Folder).WithMany(m => m.ReportSections).HasForeignKey(m => m.FolderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.ChartTemplates).WithMany(m => m.ReportSections).UsingEntity<ReportSectionChartTemplate>();

        builder.HasIndex(m => new { m.ReportId, m.Name }).IsUnique();

        base.Configure(builder);
    }
}
