namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AVOverviewInstanceConfiguration : AuditColumnsConfiguration<AVOverviewInstance>
{
    public override void Configure(EntityTypeBuilder<AVOverviewInstance> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.TemplateType).IsRequired();
        builder.Property(m => m.PublishedOn).IsRequired();
        builder.Property(m => m.IsPublished).IsRequired();
        builder.Property(m => m.Response).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.Template).WithMany(m => m.Instances).HasForeignKey(m => m.TemplateType).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.PublishedOn).IsUnique();
        builder.HasIndex(m => new { m.TemplateType, m.PublishedOn });

        base.Configure(builder);
    }
}
