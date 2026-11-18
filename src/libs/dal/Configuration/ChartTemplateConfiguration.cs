namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ChartTemplateConfiguration : BaseTypeConfiguration<ChartTemplate, int>
{
    public override void Configure(EntityTypeBuilder<ChartTemplate> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Template).IsRequired().HasColumnType("text");
        builder.Property(m => m.IsPublic).IsRequired();
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(m => m.Name).IsUnique();

        base.Configure(builder);
    }
}
