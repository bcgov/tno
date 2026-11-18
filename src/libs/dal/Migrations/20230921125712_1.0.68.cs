using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1068 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<bool>(
                name: "is_public",
                table: "report_template",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "body",
                table: "report_instance",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "sent_on",
                table: "report_instance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "subject",
                table: "report_instance",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "sent_on",
                table: "notification_instance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_template_is_public_is_enabled",
                table: "report_template",
                columns: new[] { "is_public", "is_enabled" });
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropIndex(
                name: "IX_report_template_is_public_is_enabled",
                table: "report_template");

            migrationBuilder.DropColumn(
                name: "is_public",
                table: "report_template");

            migrationBuilder.DropColumn(
                name: "body",
                table: "report_instance");

            migrationBuilder.DropColumn(
                name: "sent_on",
                table: "report_instance");

            migrationBuilder.DropColumn(
                name: "subject",
                table: "report_instance");

            migrationBuilder.DropColumn(
                name: "sent_on",
                table: "notification_instance");
            PostDown(migrationBuilder);
        }
    }
}
