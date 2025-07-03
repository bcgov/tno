using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

namespace TNO.DAL.Configuration;
public class UserUpdateHistoryConfiguration : AuditColumnsConfiguration<UserUpdateHistory>
{
    public override void Configure(EntityTypeBuilder<UserUpdateHistory> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.UserId).IsRequired();
        builder.Property(m => m.ChangeType).IsRequired();
        builder.Property(m => m.Value).IsRequired().HasMaxLength(50);
        builder.Property(m => m.DateOfChange).IsRequired();

        builder.HasOne(m => m.User).WithMany(m => m.UserUpdateHistory).HasForeignKey(m => m.UserId);

        base.Configure(builder);
    }
}
