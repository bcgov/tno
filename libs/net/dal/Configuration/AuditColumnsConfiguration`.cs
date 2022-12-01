namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public abstract class AuditColumnsConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : AuditColumns
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(m => m.CreatedBy).IsRequired().HasMaxLength(250);
        builder.Property(m => m.CreatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(m => m.UpdatedBy).IsRequired().HasMaxLength(250);
        builder.Property(m => m.UpdatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(m => m.Version).IsConcurrencyToken().HasDefaultValueSql("0");
    }
}
