namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class IngestDataLocationConfiguration : AuditColumnsConfiguration<IngestDataLocation>
{
    public override void Configure(EntityTypeBuilder<IngestDataLocation> builder)
    {
        builder.HasKey(m => new { m.IngestId, m.DataLocationId });
        builder.Property(m => m.IngestId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.DataLocationId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.Ingest).WithMany(m => m.DataLocationsManyToMany).HasForeignKey(m => m.IngestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.DataLocation).WithMany(m => m.IngestsManyToMany).HasForeignKey(m => m.DataLocationId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);

    }
}
