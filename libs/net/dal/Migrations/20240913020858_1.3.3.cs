using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _133 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<bool>(
                name: "is_synced_to_s3",
                table: "file_reference",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_synced_to_s3_on",
                table: "file_reference",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "s3_path",
                table: "file_reference",
                type: "text",
                nullable: true);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "is_synced_to_s3",
                table: "file_reference");

            migrationBuilder.DropColumn(
                name: "last_synced_to_s3_on",
                table: "file_reference");

            migrationBuilder.DropColumn(
                name: "s3_path",
                table: "file_reference");
            PostDown(migrationBuilder);
        }
    }
}
