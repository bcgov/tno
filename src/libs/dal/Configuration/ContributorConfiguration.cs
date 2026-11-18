namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContributorConfiguration : BaseTypeConfiguration<Contributor, int>
{
    public override void Configure(EntityTypeBuilder<Contributor> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.SourceId).IsRequired(false);
        builder.Property(m => m.Aliases).IsRequired(false).HasMaxLength(500);
        builder.Property(m => m.IsPress).IsRequired();
        builder.Property(m => m.AutoTranscribe).IsRequired();

        builder.HasOne(m => m.Source).WithMany(m => m.Contributors).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);

        // CREATE UNIQUE INDEX "IX_contributor_name" ON public."contributor"
        // ("name") WHERE "source_id" IS NULL;

        // CREATE UNIQUE INDEX "IX_contributor_source_id_name" ON public."contributor"
        // ("source_id", "name") WHERE "source_id" IS NOT NULL;

        base.Configure(builder);
    }
}
