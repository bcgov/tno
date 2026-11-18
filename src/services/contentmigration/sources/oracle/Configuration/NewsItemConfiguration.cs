using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// NewsItemConfiguration class, can be used for configuration of NewsItem Entity
/// </summary>
public class NewsItemConfiguration : IEntityTypeConfiguration<NewsItem>
{
    /// <summary>
    /// Configure the Entity.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public virtual void Configure(EntityTypeBuilder<NewsItem> builder)
    {
        builder.ToTable("NEWS_ITEMS", "TNO");
        builder.HasKey(x => new { x.RSN });
    }
}
