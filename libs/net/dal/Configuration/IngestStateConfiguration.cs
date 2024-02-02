namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class IngestStateConfiguration : IEntityTypeConfiguration<IngestState>
{
    public void Configure(EntityTypeBuilder<IngestState> builder)
    {
        builder.HasKey(m => m.IngestId);
        builder.Property(m => m.IngestId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.LastRanOn);
        builder.Property(m => m.CreationDateOfLastItem);
        builder.Property(m => m.FailedAttempts).HasDefaultValue(0);

        builder.HasOne(m => m.Ingest).WithOne(m => m.State).HasForeignKey<IngestState>(m => m.IngestId).OnDelete(DeleteBehavior.Cascade);
    }
}
