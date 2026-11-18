namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SentimentConfiguration : BaseTypeConfiguration<Sentiment, int>
{
    public override void Configure(EntityTypeBuilder<Sentiment> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Value).IsRequired();
        builder.Property(m => m.Rate).IsRequired();

        builder.HasIndex(m => m.Name).IsUnique();

        base.Configure(builder);
    }
}
