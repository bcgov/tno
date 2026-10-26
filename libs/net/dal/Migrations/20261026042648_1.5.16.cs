using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1516 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "after_posted_on",
                schema: "public",
                table: "orphan_scan",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_posted_on_id",
                table: "content",
                columns: new[] { "posted_on", "id" })
                .Annotation("Npgsql:CreatedConcurrently", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_content_posted_on_id",
                table: "content");

            migrationBuilder.DropColumn(
                name: "after_posted_on",
                schema: "public",
                table: "orphan_scan");
        }
    }
}
