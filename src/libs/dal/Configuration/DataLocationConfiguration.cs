namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class DataLocationConfiguration : BaseTypeConfiguration<DataLocation, int>
{
    public override void Configure(EntityTypeBuilder<DataLocation> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ConnectionId);

        builder.HasOne(m => m.Connection).WithMany(m => m.DataLocations).HasForeignKey(m => m.ConnectionId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.Name, "IX_name").IsUnique();

        base.Configure(builder);
    }
}
