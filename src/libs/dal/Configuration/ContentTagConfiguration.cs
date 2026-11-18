namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentTagConfiguration : AuditColumnsConfiguration<ContentTag>
{
    public override void Configure(EntityTypeBuilder<ContentTag> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.TagId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.TagId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.Content).WithMany(m => m.TagsManyToMany).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Tag).WithMany(m => m.ContentsManyToMany).HasForeignKey(m => m.TagId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
