using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// HNewsItemConfiguration class, can be used for configuration of NewsItem Entity
/// </summary>
public class HNewsItemConfiguration : IEntityTypeConfiguration<HNewsItem>
{
    /// <summary>
    /// Configure the Entity.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public virtual void Configure(EntityTypeBuilder<HNewsItem> builder)
    {
        builder.ToTable("HNEWS_ITEMS", "TNO");
        builder.HasKey(x => new { x.RSN });
    }
}
