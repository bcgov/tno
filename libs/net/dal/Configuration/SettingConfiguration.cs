namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SettingConfiguration : BaseTypeConfiguration<Setting, int>
{
    public override void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Value).IsRequired().HasMaxLength(100).HasDefaultValueSql("''");

        builder.HasIndex(m => m.Name, "IX_setting_name").IsUnique();

        base.Configure(builder);
    }
}
