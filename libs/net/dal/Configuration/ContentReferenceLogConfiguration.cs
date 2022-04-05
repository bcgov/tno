namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentReferenceLogConfiguration : AuditColumnsConfiguration<ContentReferenceLog>
{
    public override void Configure(EntityTypeBuilder<ContentReferenceLog> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Source).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Uid).IsRequired().HasMaxLength(100);
        builder.Property(m => m.WorkflowStatus);
        builder.Property(m => m.Message).IsRequired().HasMaxLength(2000);

        builder.HasOne(m => m.ContentReference).WithMany(m => m.Logs).HasForeignKey(m => new { m.Source, m.Uid }).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
