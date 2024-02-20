using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _10123 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropIndex(
                name: "IX_content_reference",
                table: "content_reference");

            migrationBuilder.DropColumn(
                name: "offset",
                table: "content_reference");

            migrationBuilder.DropColumn(
                name: "partition",
                table: "content_reference");

            migrationBuilder.CreateIndex(
                name: "IX_content_reference",
                table: "content_reference",
                columns: new[] { "published_on", "status" });
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropIndex(
                name: "IX_content_reference",
                table: "content_reference");

            migrationBuilder.AddColumn<long>(
                name: "offset",
                table: "content_reference",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "partition",
                table: "content_reference",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_content_reference",
                table: "content_reference",
                columns: new[] { "published_on", "partition", "offset", "status" });
            PostDown(migrationBuilder);
        }
    }
}
