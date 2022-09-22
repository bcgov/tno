namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentTypeConfiguration : BaseTypeConfiguration<Product, int>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        base.Configure(builder);
    }
}
