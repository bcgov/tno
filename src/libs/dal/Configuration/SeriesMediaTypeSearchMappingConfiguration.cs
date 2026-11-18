namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SeriesMediaTypeSearchMappingConfiguration : AuditColumnsConfiguration<SeriesMediaTypeSearchMapping>
{
    public override void Configure(EntityTypeBuilder<SeriesMediaTypeSearchMapping> builder)
    {
        builder.HasKey(m => new { m.SeriesId, m.MediaTypeId });
        builder.Property(m => m.SeriesId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.MediaTypeId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.Series).WithMany(m => m.MediaTypeSearchMappingsManyToMany).HasForeignKey(m => m.SeriesId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.MediaType).WithMany(m => m.SeriesSearchMappingsManyToMany).HasForeignKey(m => m.MediaTypeId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);

    }
}
