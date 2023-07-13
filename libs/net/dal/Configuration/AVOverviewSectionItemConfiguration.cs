namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AVOverviewSectionItemConfiguration : BaseTypeConfiguration<AVOverviewSectionItem, int>
{
    public override void Configure(EntityTypeBuilder<AVOverviewSectionItem> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AVOverviewSectionId).IsRequired();
        builder.Property(m => m.ItemType).IsRequired();
        builder.Property(m => m.Time).HasMaxLength(50);
        builder.Property(m => m.Summary).HasMaxLength(250);
        builder.Property(m => m.ContentId);

        base.Configure(builder);
    }
}