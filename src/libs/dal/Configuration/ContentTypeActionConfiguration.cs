namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentTypeActionConfiguration : AuditColumnsConfiguration<ContentTypeAction>
{
    public override void Configure(EntityTypeBuilder<ContentTypeAction> builder)
    {
        builder.HasKey(m => new { m.ContentType, m.ActionId });
        builder.Property(m => m.ContentType).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ActionId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.Action).WithMany(m => m.ContentTypes).HasForeignKey(m => m.ActionId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);

    }
}
