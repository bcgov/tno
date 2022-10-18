namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentConfiguration : AuditColumnsConfiguration<Content>
{
    public override void Configure(EntityTypeBuilder<Content> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Uid).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.ContentType).IsRequired();
        builder.Property(m => m.SourceId);
        builder.Property(m => m.OtherSource).IsRequired().HasMaxLength(100);
        builder.Property(m => m.LicenseId).IsRequired();
        builder.Property(m => m.ProductId).IsRequired();
        builder.Property(m => m.SeriesId);
        builder.Property(m => m.OwnerId);
        builder.Property(m => m.PublishedOn);
        builder.Property(m => m.SourceUrl).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Headline).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Page).IsRequired().HasMaxLength(10);
        builder.Property(m => m.Summary).IsRequired().HasColumnType("text");
        builder.Property(m => m.Body).IsRequired().HasColumnType("text");

        builder.HasOne(m => m.Source).WithMany(m => m.Contents).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Product).WithMany(m => m.Contents).HasForeignKey(m => m.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.License).WithMany(m => m.Contents).HasForeignKey(m => m.LicenseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Series).WithMany(m => m.Contents).HasForeignKey(m => m.SeriesId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Owner).WithMany(m => m.Contents).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Actions).WithMany(m => m.Contents).UsingEntity<ContentAction>();
        builder.HasMany(m => m.Categories).WithMany(m => m.Contents).UsingEntity<ContentCategory>();
        builder.HasMany(m => m.Tags).WithMany(m => m.Contents).UsingEntity<ContentTag>();
        builder.HasMany(m => m.TonePools).WithMany(m => m.Contents).UsingEntity<ContentTonePool>();

        builder.HasIndex(m => new { m.Status, m.PublishedOn, m.OtherSource, m.Uid, m.Page });

        base.Configure(builder);
    }
}
