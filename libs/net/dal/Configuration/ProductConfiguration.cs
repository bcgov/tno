namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ProductConfiguration : BaseTypeConfiguration<Product, int>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AutoTranscribe).IsRequired();
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(m => m.Name, "IX_name").IsUnique();

        base.Configure(builder);
    }
}
