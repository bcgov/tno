namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class TagConfiguration : BaseTypeConfiguration<Tag, int>
{
    public override void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(m => m.Code).IsRequired().HasMaxLength(15).ValueGeneratedNever();
        base.Configure(builder);
    }
}
