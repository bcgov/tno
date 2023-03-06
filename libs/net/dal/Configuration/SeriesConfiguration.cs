namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SeriesConfiguration : BaseTypeConfiguration<Series, int>
{
    public override void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.AutoTranscribe).IsRequired();
        builder.Property(m => m.UseInTopics).IsRequired();

        builder.HasIndex(m => m.Name, "IX_name").IsUnique();

        base.Configure(builder);
    }
}
