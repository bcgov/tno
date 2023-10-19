using System.Text.Json;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1078 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<JsonDocument>(
                name: "Settings",
                table: "product",
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
                name: "Settings",
                table: "product");
            PostDown(migrationBuilder);
        }
    }
}
