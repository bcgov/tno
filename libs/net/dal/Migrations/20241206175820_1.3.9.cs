using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _139 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<bool>(
                name: "is_cbra_unqualified",
                table: "content",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "is_cbra_unqualified",
                table: "content");
            PostDown(migrationBuilder);
        }
    }
}
