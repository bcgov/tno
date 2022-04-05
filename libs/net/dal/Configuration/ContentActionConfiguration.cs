namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentActionConfiguration : AuditColumnsConfiguration<ContentAction>
{
    public override void Configure(EntityTypeBuilder<ContentAction> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.ActionId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ActionId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Value).IsRequired();

        builder.HasOne(m => m.Content).WithMany(m => m.ActionsManyToMany).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Action).WithMany(m => m.ContentsManyToMany).HasForeignKey(m => m.ActionId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);

    }
}
