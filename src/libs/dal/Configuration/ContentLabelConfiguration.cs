namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentLabelConfiguration : AuditColumnsConfiguration<ContentLabel>
{
    public override void Configure(EntityTypeBuilder<ContentLabel> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ContentId).IsRequired();
        builder.Property(m => m.Key).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Value).IsRequired().HasMaxLength(250);

        builder.HasOne(m => m.Content).WithMany(m => m.Labels).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.Key, m.Value }, "IX_content_label");

        base.Configure(builder);
    }
}
