namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public virtual void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired();
        builder.Property(m => m.Key).IsRequired();

        builder.HasMany(m => m.Claims).WithMany(m => m.Roles).UsingEntity(m => m.ToTable("role_claim"));
    }
}