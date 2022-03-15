namespace TNO.Tools.Import.Source.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Source.Entities;

public class NewsItemConfiguration : IEntityTypeConfiguration<NewsItem>
{
  public virtual void Configure(EntityTypeBuilder<NewsItem> builder)
  {
  }
}