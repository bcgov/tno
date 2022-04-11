namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ActionConfiguration : BaseTypeConfiguration<Action, int>
{
    public override void Configure(EntityTypeBuilder<Action> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ValueType).IsRequired().HasDefaultValue(ValueType.Boolean);
        builder.Property(m => m.ValueLabel).IsRequired().HasMaxLength(100).HasDefaultValueSql("''");
        builder.Property(m => m.DefaultValue).IsRequired().HasMaxLength(100).HasDefaultValueSql("''");

        base.Configure(builder);
    }
}
