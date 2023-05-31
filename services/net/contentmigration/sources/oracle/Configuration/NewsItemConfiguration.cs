using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TNO.Services.ContentMigration.Sources.Oracle;
public class NewsItemConfiguration : IEntityTypeConfiguration<NewsItem>
{
  public virtual void Configure(EntityTypeBuilder<NewsItem> builder)
  {
  }
}
