namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class TagConfiguration : BaseTypeConfiguration<Tag, int>
{
    public override void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Code).IsRequired().HasMaxLength(15);

        builder.HasIndex(m => new { m.Code }, "IX_tag_code").IsUnique();
        builder.HasIndex(m => m.Name, "IX_tag_name").IsUnique();

        base.Configure(builder);
    }
}
