namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class MediaAnalyticsConfiguration : BaseTypeConfiguration<MediaAnalytics, int>
{
    public override void Configure(EntityTypeBuilder<MediaAnalytics> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.PublishedOn);
        builder.Property(m => m.SourceId);
        builder.Property(m => m.MediaTypeId);
        builder.Property(m => m.UniqueViews);
        builder.Property(m => m.TotalViews);
        builder.Property(m => m.AverageViews);
        builder.Property(m => m.MaleViewers);
        
        builder.Property(m => m.AgeGroup1);
        builder.Property(m => m.AgeGroup1Label).HasMaxLength(250);
        builder.Property(m => m.AgeGroup2);
        builder.Property(m => m.AgeGroup2Label).HasMaxLength(250);
        builder.Property(m => m.AgeGroup3);
        builder.Property(m => m.AgeGroup3Label).HasMaxLength(250);
        builder.Property(m => m.AgeGroup4);
        builder.Property(m => m.AgeGroup4Label).HasMaxLength(250);

        builder.Property(m => m.PageViews1);
        builder.Property(m => m.PageViews1Label).HasMaxLength(250);
        builder.Property(m => m.PageViews2);
        builder.Property(m => m.Page_Views2_Label).HasMaxLength(250);
        builder.Property(m => m.PageViews3);
        builder.Property(m => m.Page_Views3_Label).HasMaxLength(250);
        builder.Property(m => m.PageViews4);
        builder.Property(m => m.Page_Views4_Label).HasMaxLength(250);

        builder.Property(m => m.WatchTime1);
        builder.Property(m => m.WatchTime1Label).HasMaxLength(250);
        builder.Property(m => m.WatchTime2);
        builder.Property(m => m.WatchTime2Label).HasMaxLength(250);
        builder.Property(m => m.WatchTime3);
        builder.Property(m => m.WatchTime3Label).HasMaxLength(250);
        builder.Property(m => m.WatchTime4);
        builder.Property(m => m.WatchTime4Label).HasMaxLength(250);
        
        builder.HasOne(m => m.Source).WithMany().HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.MediaType).WithMany().HasForeignKey(m => m.MediaTypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(m => new { m.PublishedOn, m.SourceId, m.MediaTypeId })
           .IsUnique();
                
        base.Configure(builder);
    }
}
