namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportAIResultConfiguration : AuditColumnsConfiguration<ReportAIResult>
{
    public override void Configure(EntityTypeBuilder<ReportAIResult> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Hash).IsRequired().HasMaxLength(64);
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.Manifest).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.Output).IsRequired().HasColumnType("text");
        builder.Property(m => m.Error).HasColumnType("text");
        builder.Property(m => m.PipelineVersion).IsRequired().HasMaxLength(20);

        builder.HasOne(m => m.Report).WithMany().HasForeignKey(m => m.ReportId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.ReportInstance).WithMany().HasForeignKey(m => m.ReportInstanceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.ReportSection).WithMany().HasForeignKey(m => m.ReportSectionId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.Hash, "IX_report_ai_result_hash").IsUnique();
        builder.HasIndex(m => m.CreatedOn, "IX_report_ai_result_created_on");

        base.Configure(builder);
    }
}
