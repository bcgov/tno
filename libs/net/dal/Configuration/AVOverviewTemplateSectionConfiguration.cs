namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AVOverviewTemplateSectionConfiguration : BaseTypeConfiguration<AVOverviewTemplateSection, int>
{
    public override void Configure(EntityTypeBuilder<AVOverviewTemplateSection> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AVOverviewTemplateId).IsRequired();
        builder.Property(m => m.SourceId);
        builder.Property(m => m.OtherSource);
        builder.Property(m => m.SeriesId);
        builder.Property(m => m.Anchors);
        builder.Property(m => m.StartTime);
        
        base.Configure(builder);
    }
}