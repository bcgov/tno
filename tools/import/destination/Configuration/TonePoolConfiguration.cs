namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class TonePoolConfiguration : IEntityTypeConfiguration<TonePool>
{
    public virtual void Configure(EntityTypeBuilder<TonePool> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired();

        builder.HasOne(m => m.Owner).WithMany(m => m.TonePools).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}