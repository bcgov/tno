namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class TonePoolConfiguration : BaseTypeConfiguration<TonePool, int>
{
    public override void Configure(EntityTypeBuilder<TonePool> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.OwnerId).IsRequired();
        builder.Property(m => m.IsPublic).IsRequired();

        builder.HasOne(m => m.Owner).WithMany(m => m.TonePools).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
