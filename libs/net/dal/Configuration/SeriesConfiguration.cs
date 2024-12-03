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
        builder.Property(m => m.IsCBRASource);

        builder.HasOne(m => m.Source).WithMany(m => m.Series).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.MediaTypeSearchMappings).WithMany(m => m.SeriesSearchMappings).UsingEntity<SeriesMediaTypeSearchMapping>();

        // CREATE UNIQUE INDEX "IX_series_name" ON public."series"
        // ("name") WHERE "source_id" IS NULL;

        // CREATE UNIQUE INDEX "IX_source_id_name" ON public."series"
        // ("source_id", "name") WHERE "source_id" IS NOT NULL;

        base.Configure(builder);
    }
}
