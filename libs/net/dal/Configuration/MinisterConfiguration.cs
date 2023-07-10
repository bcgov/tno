namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class MinisterConfiguration : BaseTypeConfiguration<Minister, int>
{
    public override void Configure(EntityTypeBuilder<Minister> builder)
    {
        builder.Property(m => m.Aliases).HasDefaultValueSql("''").HasMaxLength(250);
        base.Configure(builder);
    }
}
