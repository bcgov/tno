namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SourceConfiguration : BaseTypeConfiguration<Source, int>
{
    public override void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Code).IsRequired().HasMaxLength(20);
        builder.Property(m => m.ShortName).IsRequired().HasMaxLength(100).HasDefaultValueSql("''");
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000).HasDefaultValueSql("''");
        builder.Property(m => m.OwnerId);
        builder.Property(m => m.LicenseId).IsRequired();
        builder.Property(m => m.AutoTranscribe).IsRequired();
        builder.Property(m => m.DisableTranscribe).IsRequired();

        builder.HasOne(m => m.Owner).WithMany().HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.License).WithMany(m => m.Sources).HasForeignKey(m => m.LicenseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(m => m.Actions).WithMany(m => m.Sources).UsingEntity<SourceSourceAction>();
        builder.HasMany(m => m.Metrics).WithMany(m => m.Sources).UsingEntity<SourceMetric>();

        builder.HasIndex(m => m.Name).IsUnique();
        builder.HasIndex(m => m.Code).IsUnique();

        base.Configure(builder);
    }
}
