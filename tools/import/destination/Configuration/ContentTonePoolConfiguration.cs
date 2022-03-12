namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class ContentTonePoolConfiguration : IEntityTypeConfiguration<ContentTonePool>
{
    public virtual void Configure(EntityTypeBuilder<ContentTonePool> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.TonePoolId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.TonePoolId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Value).IsRequired();

        builder.HasOne(m => m.Content).WithMany(m => m.TonePools).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.TonePool).WithMany(m => m.ContentTonePools).HasForeignKey(m => m.TonePoolId).OnDelete(DeleteBehavior.Cascade);

    }
}