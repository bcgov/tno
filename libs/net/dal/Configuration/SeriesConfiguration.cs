namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SeriesConfiguration : BaseTypeConfiguration<Series, int>
{
    public override void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.SourceId).IsRequired(false);
        builder.Property(m => m.AutoTranscribe).IsRequired();
        builder.Property(m => m.UseInTopics).IsRequired();

        builder.HasOne(m => m.Source).WithMany(m => m.Series).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
