namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportInstanceConfiguration : AuditColumnsConfiguration<ReportInstance>
{
    public override void Configure(EntityTypeBuilder<ReportInstance> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ReportId);
        builder.Property(m => m.PublishedOn);

        builder.HasOne(m => m.Report).WithMany(m => m.Instances).HasForeignKey(m => m.ReportId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.Content).WithMany(m => m.Reports).UsingEntity<ReportInstanceContent>();

        builder.HasIndex(m => new { m.PublishedOn, m.CreatedOn }, "IX_report_dates");

        base.Configure(builder);
    }
}
