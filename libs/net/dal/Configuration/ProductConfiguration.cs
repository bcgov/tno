namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ProductConfiguration : BaseTypeConfiguration<Product, int>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        builder.HasIndex(m => m.Name, "IX_name").IsUnique();

        base.Configure(builder);
    }
}
