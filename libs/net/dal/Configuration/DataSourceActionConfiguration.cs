namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class DataSourceActionConfiguration : AuditColumnsConfiguration<DataSourceAction>
{
    public override void Configure(EntityTypeBuilder<DataSourceAction> builder)
    {
        builder.HasKey(m => new { m.DataSourceId, m.SourceActionId });
        builder.Property(m => m.DataSourceId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SourceActionId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Value).IsRequired().HasMaxLength(250);

        builder.HasOne(m => m.DataSource).WithMany(m => m.ActionsManyToMany).HasForeignKey(m => m.DataSourceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.SourceAction).WithMany(m => m.DataSourcesManyToMany).HasForeignKey(m => m.SourceActionId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);

    }
}
