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
        builder.Property(m => m.Status);
        builder.Property(m => m.WorkflowStatus);
        builder.Property(m => m.ContentTypeId);
        builder.Property(m => m.MediaTypeId);
        builder.Property(m => m.LicenseId);
        builder.Property(m => m.SeriesId);
        builder.Property(m => m.OwnerId);
        builder.Property(m => m.DataSourceId);
        builder.Property(m => m.Source).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Headline).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Uid).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Page).IsRequired().HasMaxLength(10);
        builder.Property(m => m.PublishedOn);
        builder.Property(m => m.Summary).IsRequired().HasColumnType("text");
        builder.Property(m => m.Transcription).IsRequired().HasColumnType("text");
        builder.Property(m => m.SourceUrl).IsRequired().HasMaxLength(500);

        builder.HasOne(m => m.ContentType).WithMany(m => m.Contents).HasForeignKey(m => m.ContentTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.MediaType).WithMany(m => m.Contents).HasForeignKey(m => m.MediaTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.License).WithMany(m => m.Contents).HasForeignKey(m => m.LicenseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Series).WithMany(m => m.Contents).HasForeignKey(m => m.SeriesId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.DataSource).WithMany(m => m.Contents).HasForeignKey(m => m.DataSourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Owner).WithMany(m => m.Contents).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Actions).WithMany(m => m.Contents).UsingEntity<ContentAction>();
        builder.HasMany(m => m.Categories).WithMany(m => m.Contents).UsingEntity<ContentCategory>();
        builder.HasMany(m => m.Tags).WithMany(m => m.Contents).UsingEntity<ContentTag>();
        builder.HasMany(m => m.TonePools).WithMany(m => m.Contents).UsingEntity<ContentTonePool>();

        builder.HasIndex(m => new { m.Status, m.WorkflowStatus, m.PublishedOn, m.Source, m.Uid, m.Page });

        base.Configure(builder);
    }
}
