namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class ContentTagConfiguration : IEntityTypeConfiguration<ContentTag>
{
    public virtual void Configure(EntityTypeBuilder<ContentTag> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.TagId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.TagId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.Content).WithMany().HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Tag).WithMany().HasForeignKey(m => m.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}