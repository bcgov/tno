namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserColleagueConfiguration : AuditColumnsConfiguration<UserColleague>
{
    public override void Configure(EntityTypeBuilder<UserColleague> builder)
    {
        builder.HasKey(m => new { m.UserId, m.ColleagueId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ColleagueId).IsRequired().ValueGeneratedNever();

        base.Configure(builder);
    }
}
