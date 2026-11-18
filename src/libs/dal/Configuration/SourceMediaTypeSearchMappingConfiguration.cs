namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SourceMediaTypeSearchMappingConfiguration : AuditColumnsConfiguration<SourceMediaTypeSearchMapping>
{
    public override void Configure(EntityTypeBuilder<SourceMediaTypeSearchMapping> builder)
    {
        builder.HasKey(m => new { m.SourceId, m.MediaTypeId });
        builder.Property(m => m.SourceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.MediaTypeId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.Source).WithMany(m => m.MediaTypeSearchMappingsManyToMany).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.MediaType).WithMany(m => m.SourceSearchMappingsManyToMany).HasForeignKey(m => m.MediaTypeId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);

    }
}
