namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentTypeConfiguration : BaseTypeConfiguration<ContentType, int>
{
    public override void Configure(EntityTypeBuilder<ContentType> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        builder.HasMany(m => m.Actions).WithMany(m => m.ContentTypes).UsingEntity<ContentTypeAction>();

        base.Configure(builder);
    }
}
