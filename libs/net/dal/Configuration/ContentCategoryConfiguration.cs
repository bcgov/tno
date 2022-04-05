namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentCategoryConfiguration : AuditColumnsConfiguration<ContentCategory>
{
    public override void Configure(EntityTypeBuilder<ContentCategory> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.CategoryId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.CategoryId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Score).IsRequired();

        builder.HasOne(m => m.Content).WithMany(m => m.CategoriesManyToMany).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Category).WithMany(m => m.ContentsManyToMany).HasForeignKey(m => m.CategoryId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
