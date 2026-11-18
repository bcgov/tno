using System;
using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1065 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder, true);
            migrationBuilder.AddColumn<DateTime>(
                name: "posted_on",
                table: "content",
                type: "timestamp with time zone",
                nullable: true);
            PostUp(migrationBuilder, true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder, true);
            migrationBuilder.DropColumn(
                name: "posted_on",
                table: "content");
            PostDown(migrationBuilder, true);
        }
    }
}
