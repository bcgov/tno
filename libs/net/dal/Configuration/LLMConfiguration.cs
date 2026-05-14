namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class LLMConfiguration : BaseTypeConfiguration<LLM, int>
{
    public override void Configure(EntityTypeBuilder<LLM> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.IsPublic).IsRequired().HasDefaultValue(ValueType.Boolean);
        builder.Property(m => m.DeploymentName).IsRequired().HasMaxLength(150);
        builder.Property(m => m.AgentName).HasMaxLength(150);
        builder.Property(m => m.SystemPrompt).IsRequired().HasColumnType("text");
        builder.Property(m => m.UserPrompt).IsRequired().HasColumnType("text");
        builder.Property(m => m.ApiKey).HasMaxLength(500);
        builder.Property(m => m.ProjectEndpoint).HasMaxLength(1000);

        builder.HasIndex(m => m.Name, "IX_ai_model").IsUnique();
        builder.HasIndex(m => new { m.IsPublic }, "IX_ai_model_search");

        base.Configure(builder);
    }
}
