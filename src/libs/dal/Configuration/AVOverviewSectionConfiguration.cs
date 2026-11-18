namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AVOverviewSectionConfiguration : AuditColumnsConfiguration<AVOverviewSection>
{
    public override void Configure(EntityTypeBuilder<AVOverviewSection> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.InstanceId).IsRequired();
        builder.Property(m => m.SourceId);
        builder.Property(m => m.OtherSource).IsRequired().HasMaxLength(100);
        builder.Property(m => m.SeriesId);
        builder.Property(m => m.Anchors).IsRequired().HasMaxLength(100);
        builder.Property(m => m.StartTime).IsRequired().HasMaxLength(8);
        builder.Property(m => m.SortOrder).IsRequired();

        builder.HasOne(m => m.Instance).WithMany(m => m.Sections).HasForeignKey(m => m.InstanceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Source).WithMany().HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Series).WithMany().HasForeignKey(m => m.SeriesId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
