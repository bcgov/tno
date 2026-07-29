using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _153 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "create_clone",
                table: "automation_action",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "create_identifier",
                table: "automation_action",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "settings",
                table: "automation_action",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<string>(
                name: "works_on",
                table: "automation_action",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "create_clone",
                table: "automation_action");

            migrationBuilder.DropColumn(
                name: "create_identifier",
                table: "automation_action");

            migrationBuilder.DropColumn(
                name: "settings",
                table: "automation_action");

            migrationBuilder.DropColumn(
                name: "works_on",
                table: "automation_action");
        }
    }
}
