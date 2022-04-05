namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentLogConfiguration : AuditColumnsConfiguration<ContentLog>
{
    public override void Configure(EntityTypeBuilder<ContentLog> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ContentId).IsRequired();
        builder.Property(m => m.Status);
        builder.Property(m => m.WorkflowStatus);
        builder.Property(m => m.Message).IsRequired().HasMaxLength(2000);

        builder.HasOne(m => m.Content).WithMany(m => m.Logs).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
