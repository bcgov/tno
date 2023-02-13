namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public abstract class BaseTypeConfiguration<TBaseEntity, TKey> : AuditColumnsConfiguration<TBaseEntity>
    where TBaseEntity : BaseType<TKey>
    where TKey : notnull
{
    public override void Configure(EntityTypeBuilder<TBaseEntity> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000).HasDefaultValueSql("''");
        builder.Property(m => m.IsEnabled);
        builder.Property(m => m.SortOrder).HasDefaultValue(0);

        builder.HasIndex(m => m.Name, "IX_name").IsUnique();

        base.Configure(builder);
    }
}
