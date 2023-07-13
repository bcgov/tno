namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AVOverviewInstanceConfiguration : BaseTypeConfiguration<AVOverviewInstance, int>
{
    public override void Configure(EntityTypeBuilder<AVOverviewInstance> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.TemplateType).IsRequired();
        builder.Property(m => m.PublishedOn);
        builder.Property(m => m.Response).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        base.Configure(builder);
    }
}