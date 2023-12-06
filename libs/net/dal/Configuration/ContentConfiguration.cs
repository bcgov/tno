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
        builder.Property(m => m.ExternalUid).HasMaxLength(500).HasDefaultValue(string.Empty);
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.ContentType).IsRequired();
        builder.Property(m => m.SourceId);
        builder.Property(m => m.OtherSource).IsRequired().HasMaxLength(100);
        builder.Property(m => m.LicenseId).IsRequired();
        builder.Property(m => m.MediaTypeId).IsRequired();
        builder.Property(m => m.SeriesId);
        builder.Property(m => m.ContributorId);
        builder.Property(m => m.Edition).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Section).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Byline).IsRequired().HasMaxLength(500);
        builder.Property(m => m.OwnerId);
        builder.Property(m => m.PostedOn);
        builder.Property(m => m.PublishedOn);
        builder.Property(m => m.IsHidden).IsRequired();
        builder.Property(m => m.IsApproved).IsRequired();
        builder.Property(m => m.IsPrivate).IsRequired();
        builder.Property(m => m.SourceUrl).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Headline).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Page).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Summary).IsRequired().HasColumnType("text");
        builder.Property(m => m.Body).IsRequired().HasColumnType("text");
        builder.Property(m => m.Versions).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.Source).WithMany(m => m.Contents).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.MediaType).WithMany(m => m.Contents).HasForeignKey(m => m.MediaTypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.License).WithMany(m => m.Contents).HasForeignKey(m => m.LicenseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Series).WithMany(m => m.Contents).HasForeignKey(m => m.SeriesId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Contributor).WithMany(m => m.Contents).HasForeignKey(m => m.ContributorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Owner).WithMany(m => m.Contents).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(m => m.Actions).WithMany(m => m.Contents).UsingEntity<ContentAction>();
        builder.HasMany(m => m.Topics).WithMany(m => m.Contents).UsingEntity<ContentTopic>();
        builder.HasMany(m => m.Tags).WithMany(m => m.Contents).UsingEntity<ContentTag>();
        builder.HasMany(m => m.TonePools).WithMany(m => m.Contents).UsingEntity<ContentTonePool>();

        builder.HasIndex(m => new { m.PublishedOn, m.CreatedOn }, "IX_content_dates");
        builder.HasIndex(m => new { m.ContentType, m.OtherSource, m.Uid, m.Page, m.Status, m.IsHidden }, "IX_content");
        builder.HasIndex(m => new { m.Edition, m.Section, m.Byline }, "IX_print_content");
        builder.HasIndex(m => m.Headline, "IX_headline");

        base.Configure(builder);
    }
}
