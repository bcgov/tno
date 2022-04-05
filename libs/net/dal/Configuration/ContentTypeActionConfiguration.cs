namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentTypeActionConfiguration : AuditColumnsConfiguration<ContentTypeAction>
{
    public override void Configure(EntityTypeBuilder<ContentTypeAction> builder)
    {
        builder.HasKey(m => new { m.ContentTypeId, m.ActionId });
        builder.Property(m => m.ContentTypeId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ActionId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.ContentType).WithMany(m => m.ActionsManyToMany).HasForeignKey(m => m.ContentTypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Action).WithMany(m => m.ContentTypesManyToMany).HasForeignKey(m => m.ActionId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);

    }
}
