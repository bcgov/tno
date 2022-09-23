namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class CategoryConfiguration : BaseTypeConfiguration<Category, int>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.CategoryType).IsRequired();
        builder.Property(m => m.AutoTranscribe).IsRequired();

        base.Configure(builder);
    }
}
