namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class ContentTypeConfiguration : IEntityTypeConfiguration<ContentType>
{
    public virtual void Configure(EntityTypeBuilder<ContentType> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired();

        builder.HasMany(m => m.Actions).WithMany(m => m.ContentTypes).UsingEntity(m => m.ToTable("content_type_action"));
    }
}