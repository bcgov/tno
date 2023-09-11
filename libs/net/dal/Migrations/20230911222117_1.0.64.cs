using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1064 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<int>(
                name: "notification_id",
                table: "event_schedule",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "report_id",
                table: "event_schedule",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "request_sent_on",
                table: "event_schedule",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_schedule_notification_id",
                table: "event_schedule",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_schedule_report_id",
                table: "event_schedule",
                column: "report_id");

            migrationBuilder.AddForeignKey(
                name: "FK_event_schedule_notification_notification_id",
                table: "event_schedule",
                column: "notification_id",
                principalTable: "notification",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_event_schedule_report_report_id",
                table: "event_schedule",
                column: "report_id",
                principalTable: "report",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_event_schedule_notification_notification_id",
                table: "event_schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_event_schedule_report_report_id",
                table: "event_schedule");

            migrationBuilder.DropIndex(
                name: "IX_event_schedule_notification_id",
                table: "event_schedule");

            migrationBuilder.DropIndex(
                name: "IX_event_schedule_report_id",
                table: "event_schedule");

            migrationBuilder.DropColumn(
                name: "notification_id",
                table: "event_schedule");

            migrationBuilder.DropColumn(
                name: "report_id",
                table: "event_schedule");

            migrationBuilder.DropColumn(
                name: "request_sent_on",
                table: "event_schedule");
            PostDown(migrationBuilder);
        }
    }
}
