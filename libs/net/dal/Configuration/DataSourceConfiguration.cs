namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class DataSourceConfiguration : AuditColumnsConfiguration<DataSource>
{
    public override void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Code).IsRequired().HasMaxLength(20);
        builder.Property(m => m.ShortName).IsRequired().HasMaxLength(100).HasDefaultValueSql("''");
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000).HasDefaultValueSql("''");
        builder.Property(m => m.DataLocationId).IsRequired();
        builder.Property(m => m.MediaTypeId).IsRequired();
        builder.Property(m => m.LicenseId).IsRequired();
        builder.Property(m => m.ContentTypeId).IsRequired();
        builder.Property(m => m.Topic).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Connection).IsRequired().HasColumnType("json");
        builder.Property(m => m.ParentId);
        builder.Property(m => m.RetryLimit).HasDefaultValue(3);
        builder.Property(m => m.ScheduleType).HasDefaultValue(DataSourceScheduleType.None);

        builder.HasOne(m => m.Owner).WithMany().HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.ContentType).WithMany(m => m.DataSources).HasForeignKey(m => m.ContentTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.DataLocation).WithMany(m => m.DataSources).HasForeignKey(m => m.DataLocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.MediaType).WithMany(m => m.DataSources).HasForeignKey(m => m.MediaTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.License).WithMany(m => m.DataSources).HasForeignKey(m => m.LicenseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Parent).WithMany().HasForeignKey(m => m.ParentId).OnDelete(DeleteBehavior.ClientSetNull);
        builder.HasMany(m => m.Actions).WithMany(m => m.DataSources).UsingEntity<DataSourceAction>();
        builder.HasMany(m => m.Metrics).WithMany(m => m.DataSources).UsingEntity<DataSourceMetric>();
        builder.HasMany(m => m.Schedules).WithMany(m => m.DataSources).UsingEntity<DataSourceSchedule>();

        builder.HasIndex(m => m.Name).IsUnique();
        builder.HasIndex(m => m.Code).IsUnique();

        base.Configure(builder);
    }
}
