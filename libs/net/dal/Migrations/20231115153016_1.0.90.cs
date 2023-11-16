using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1090 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);

            migrationBuilder.AddColumn<bool>(
                name: "is_private",
                table: "content",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Dictionary<int, Entities.Models.ContentVersion>>(
                name: "versions",
                table: "content",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);

            migrationBuilder.DropColumn(
                name: "is_private",
                table: "content");

            migrationBuilder.DropColumn(
                name: "versions",
                table: "content");

            PostDown(migrationBuilder);
        }
    }
}
