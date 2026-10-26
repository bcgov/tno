using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1515 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "end_posted_on",
                schema: "public",
                table: "orphan_scan",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "start_posted_on",
                schema: "public",
                table: "orphan_scan",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_posted_on",
                schema: "public",
                table: "orphan_scan");

            migrationBuilder.DropColumn(
                name: "start_posted_on",
                schema: "public",
                table: "orphan_scan");
        }
    }
}
