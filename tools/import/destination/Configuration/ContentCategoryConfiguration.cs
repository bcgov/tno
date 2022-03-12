namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class ContentCategoryConfiguration : IEntityTypeConfiguration<ContentCategory>
{
    public virtual void Configure(EntityTypeBuilder<ContentCategory> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.CategoryId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.CategoryId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Score).IsRequired();

        builder.HasOne(m => m.Content).WithMany(m => m.Categories).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Category).WithMany(m => m.ContentCategories).HasForeignKey(m => m.CategoryId).OnDelete(DeleteBehavior.Cascade);

    }
}