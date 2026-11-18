using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1315 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.CreateIndex(
                name: "IX_content_published_on",
                table: "content",
                column: "published_on",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_content_published_on_status",
                table: "content",
                columns: new[] { "published_on", "status" },
                descending: new[] { true, false });
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropIndex(
                name: "IX_content_published_on",
                table: "content");

            migrationBuilder.DropIndex(
                name: "IX_content_published_on_status",
                table: "content");
            PostDown(migrationBuilder);
        }
    }
}
