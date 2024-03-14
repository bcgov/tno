namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class QuoteConfiguration : AuditColumnsConfiguration<Quote>
{
    public override void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ContentId).IsRequired();

        builder.Property(m => m.Byline).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Statement).IsRequired().HasColumnType("text");

        builder.HasOne(m => m.Content).WithMany(m => m.Quotes).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.Statement, "IX_statement");

        base.Configure(builder);
    }
}
