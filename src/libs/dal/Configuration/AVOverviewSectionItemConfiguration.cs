namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AVOverviewSectionItemConfiguration : AuditColumnsConfiguration<AVOverviewSectionItem>
{
    public override void Configure(EntityTypeBuilder<AVOverviewSectionItem> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.SectionId).IsRequired();
        builder.Property(m => m.ItemType).IsRequired();
        builder.Property(m => m.Time).IsRequired().HasMaxLength(8);
        builder.Property(m => m.Summary).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.ContentId);
        builder.Property(m => m.SortOrder).IsRequired();

        builder.HasOne(m => m.Section).WithMany(m => m.Items).HasForeignKey(m => m.SectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Content).WithMany().HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
