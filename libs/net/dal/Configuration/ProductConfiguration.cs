namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ProductConfiguration : BaseTypeConfiguration<Product, int>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.TargetProductId).IsRequired();
        builder.Property(m => m.ProductType).IsRequired();
        builder.Property(m => m.IsPublic).IsRequired();

        builder.HasMany(m => m.Subscribers).WithMany(m => m.ProductSubscriptions).UsingEntity<UserProduct>();

        builder.HasIndex(m => new { m.Name, m.TargetProductId, m.ProductType }).IsUnique();

        base.Configure(builder);
    }
}
