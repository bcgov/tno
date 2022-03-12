namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class ContentConfiguration : IEntityTypeConfiguration<Content>
{
    public virtual void Configure(EntityTypeBuilder<Content> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        builder.HasOne(m => m.ContentType).WithMany(m => m.Contents).HasForeignKey(m => m.ContentTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.MediaType).WithMany(m => m.Contents).HasForeignKey(m => m.MediaTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.License).WithMany(m => m.Contents).HasForeignKey(m => m.LicenseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Series).WithMany(m => m.Contents).HasForeignKey(m => m.SeriesId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.DataSource).WithMany(m => m.Contents).HasForeignKey(m => m.DataSourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Owner).WithMany(m => m.Contents).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(m => m.Tags).WithMany(m => m.Contents).UsingEntity<ContentTag>();
    }
}