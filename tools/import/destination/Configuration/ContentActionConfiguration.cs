namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class ContentActionConfiguration : IEntityTypeConfiguration<ContentAction>
{
    public virtual void Configure(EntityTypeBuilder<ContentAction> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.ActionId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ActionId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Value).IsRequired();

        builder.HasOne(m => m.Content).WithMany(m => m.Actions).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Action).WithMany(m => m.ContentActions).HasForeignKey(m => m.ActionId).OnDelete(DeleteBehavior.Cascade);

    }
}