using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// NewsItemConfiguration class, can be used for configuration of NewsItem Entity
/// </summary>
public class AllNewsItemConfiguration : IEntityTypeConfiguration<AllNewsItem>
{
    /// <summary>
    /// Configure the Entity.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public virtual void Configure(EntityTypeBuilder<AllNewsItem> builder)
    {
        builder.HasKey(x => new { x.RSN });
        builder.ToSqlQuery(@"WITH ATN AS (
            SELECT * from TNO.NEWS_ITEMS
            UNION ALL
            SELECT * from TNO.HNEWS_ITEMS
            ) SELECT * FROM ATN");
    }
}
