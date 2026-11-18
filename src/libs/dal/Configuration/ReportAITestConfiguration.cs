namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportAITestConfiguration : AuditColumnsConfiguration<ReportAITest>
{
    public override void Configure(EntityTypeBuilder<ReportAITest> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.Output).HasColumnType("text");
        builder.Property(m => m.Error).HasColumnType("text");
        builder.Property(m => m.Transcript).IsRequired().HasColumnType("text");

        builder.HasOne(m => m.Report).WithMany().HasForeignKey(m => m.ReportId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.ReportInstance).WithMany().HasForeignKey(m => m.ReportInstanceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.ReportSection).WithMany().HasForeignKey(m => m.ReportSectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Requestor).WithMany().HasForeignKey(m => m.RequestorId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.RequestorId, m.ReportSectionId }, "IX_report_ai_test_requestor_section");
        builder.HasIndex(m => m.CreatedOn, "IX_report_ai_test_created_on");

        base.Configure(builder);
    }
}
