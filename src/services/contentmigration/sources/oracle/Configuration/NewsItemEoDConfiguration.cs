using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// NewsItemEoDConfiguration class, can be used for configuration of NewsItemEoD Entity
/// </summary>
public class NewsItemEoDConfiguration : IEntityTypeConfiguration<NewsItemEoD>
{
    /// <summary>
    /// Configure the Entity.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public virtual void Configure(EntityTypeBuilder<NewsItemEoD> builder)
    {
        builder.ToTable("NEWS_ITEM_EOD", "TNO");
        builder.HasKey(x => new { x.ItemRSN });

        builder.HasOne(m => m.NewsItem).WithMany(m => m.Topics).HasForeignKey(m => m.ItemRSN);
        builder.HasOne(m => m.HNewsItem).WithMany(m => m.Topics).HasForeignKey(m => m.ItemRSN);
        builder.HasOne(m => m.AllNewsItem).WithMany(m => m.Topics).HasForeignKey(m => m.ItemRSN);
    }
}
