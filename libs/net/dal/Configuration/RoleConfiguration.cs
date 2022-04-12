namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class RoleConfiguration : AuditColumnsConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000).HasDefaultValueSql("''");
        builder.Property(m => m.Key).IsRequired();
        builder.Property(m => m.IsEnabled);

        builder.HasMany(m => m.Claims).WithMany(m => m.Roles).UsingEntity<RoleClaim>();

        builder.HasIndex(m => m.Name).IsUnique();
        builder.HasIndex(m => m.Key).IsUnique();

        base.Configure(builder);
    }
}
