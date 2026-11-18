using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

namespace TNO.DAL.Configuration;

public class OrphanScanConfiguration : IEntityTypeConfiguration<OrphanScan>
{
    public void Configure(EntityTypeBuilder<OrphanScan> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Side).HasMaxLength(20);
        builder.Property(x => x.Index).HasMaxLength(20);
        builder.Property(x => x.Status).HasMaxLength(20);
        builder.HasIndex(x => x.CreatedOn);
        // One active reconciliation across every API replica.
        builder.HasIndex(x => x.WorkerSlot).IsUnique()
            .HasFilter("completed_on IS NULL");
    }
}

public class OrphanScanResultConfiguration : IEntityTypeConfiguration<OrphanScanResult>
{
    public void Configure(EntityTypeBuilder<OrphanScanResult> builder)
    {
        builder.HasKey(x => new { x.ScanId, x.ContentId });
        builder.HasOne<OrphanScan>().WithMany().HasForeignKey(x => x.ScanId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ScanId, x.Confirmed, x.ContentId });
        builder.HasIndex(x => new { x.ScanId, x.Confirmed, x.DetectedOn });
    }
}
