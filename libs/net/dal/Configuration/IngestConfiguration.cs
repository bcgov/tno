namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class IngestConfiguration : AuditColumnsConfiguration<Ingest>
{
    public override void Configure(EntityTypeBuilder<Ingest> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Topic).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000).HasDefaultValueSql("''");
        builder.Property(m => m.IngestTypeId).IsRequired();
        builder.Property(m => m.SourceId).IsRequired();
        builder.Property(m => m.ProductId).IsRequired();
        builder.Property(m => m.Configuration).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(m => m.RetryLimit).IsRequired().HasDefaultValue(3);
        builder.Property(m => m.ScheduleType).HasDefaultValue(ScheduleType.None);
        builder.Property(m => m.SourceConnectionId).IsRequired();
        builder.Property(m => m.DestinationConnectionId).IsRequired();

        builder.HasOne(m => m.IngestType).WithMany(m => m.Ingests).HasForeignKey(m => m.IngestTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Product).WithMany(m => m.Ingests).HasForeignKey(m => m.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Source).WithMany(m => m.Ingests).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.SourceConnection).WithMany(m => m.SourceIngests).HasForeignKey(m => m.SourceConnectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.DestinationConnection).WithMany(m => m.DestinationIngests).HasForeignKey(m => m.DestinationConnectionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(m => m.Schedules).WithMany(m => m.Ingests).UsingEntity<IngestSchedule>();
        builder.HasMany(m => m.DataLocations).WithMany(m => m.Ingests).UsingEntity<IngestDataLocation>();

        builder.HasIndex(m => m.Name, "IX_name").IsUnique();
        builder.HasIndex(m => new { m.IngestTypeId, m.SourceId, m.Topic }, "IX_ingest");

        base.Configure(builder);
    }
}
