namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public virtual void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Username).IsRequired();
        builder.Property(m => m.Email).IsRequired();
        builder.Property(m => m.Key).IsRequired();

        builder.HasMany(m => m.Roles).WithMany(m => m.Users).UsingEntity(m => m.ToTable("user_role"));
    }
}