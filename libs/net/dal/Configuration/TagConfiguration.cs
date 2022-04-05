namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class TagConfiguration : BaseTypeConfiguration<Tag, string>
{
    public override void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(m => m.Id).IsRequired().HasMaxLength(6).ValueGeneratedNever();

        base.Configure(builder);
    }
}
