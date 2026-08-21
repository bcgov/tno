namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AutomationProfileConfiguration : BaseTypeConfiguration<AutomationProfile, int>
{
    public override void Configure(EntityTypeBuilder<AutomationProfile> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.SchemaVersion).IsRequired().HasDefaultValue(1);
        // The v2 definition document; nullable so v1 profiles carry no document at all.
        builder.Property(m => m.Definition).HasColumnType("jsonb");
        builder.Property(m => m.LLMId);

        // Optional references; removing a filter/LLM should not delete the profile.
        builder.HasOne(m => m.LLM).WithMany().HasForeignKey(m => m.LLMId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.Name).IsUnique();

        base.Configure(builder);
    }
}
