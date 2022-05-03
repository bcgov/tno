namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class CacheConfiguration : AuditColumnsConfiguration<Cache>
{
    public override void Configure(EntityTypeBuilder<Cache> builder)
    {
        builder.HasKey(m => m.Key);
        builder.Property(m => m.Key).IsRequired().HasMaxLength(100).ValueGeneratedNever();
        builder.Property(m => m.Value).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000).HasDefaultValueSql("''");

        base.Configure(builder);
    }
}
