namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SourceSourceActionConfiguration : AuditColumnsConfiguration<SourceSourceAction>
{
    public override void Configure(EntityTypeBuilder<SourceSourceAction> builder)
    {
        builder.HasKey(m => new { m.SourceId, m.SourceActionId });
        builder.Property(m => m.SourceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SourceActionId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Value).IsRequired().HasMaxLength(250);

        builder.HasOne(m => m.Source).WithMany(m => m.ActionsManyToMany).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.SourceAction).WithMany(m => m.SourcesManyToMany).HasForeignKey(m => m.SourceActionId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);

    }
}
