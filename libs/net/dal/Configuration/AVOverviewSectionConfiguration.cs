namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AVOverviewSectionConfiguration : BaseTypeConfiguration<AVOverviewSection, int>
{
    public override void Configure(EntityTypeBuilder<AVOverviewSection> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AVOverviewTemplateId).IsRequired();
        builder.Property(m => m.SourceId);
        builder.Property(m => m.OtherSource).HasMaxLength(50);
        builder.Property(m => m.SeriesId);
        builder.Property(m => m.Anchors).HasMaxLength(250);
        builder.Property(m => m.StartTime).HasMaxLength(50);
        builder.Property(m => m.AVOverviewInstanceId);

        base.Configure(builder);
    }
}