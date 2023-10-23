using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1079 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_folder_schedule_schedule_id",
                table: "folder");

            migrationBuilder.DropIndex(
                name: "IX_folder_schedule_id",
                table: "folder");

            migrationBuilder.DropColumn(
                name: "schedule_id",
                table: "folder");

            migrationBuilder.AddColumn<int>(
                name: "folder_id",
                table: "event_schedule",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_schedule_folder_id",
                table: "event_schedule",
                column: "folder_id");

            migrationBuilder.AddForeignKey(
                name: "FK_event_schedule_folder_folder_id",
                table: "event_schedule",
                column: "folder_id",
                principalTable: "folder",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_event_schedule_folder_folder_id",
                table: "event_schedule");

            migrationBuilder.DropIndex(
                name: "IX_event_schedule_folder_id",
                table: "event_schedule");

            migrationBuilder.DropColumn(
                name: "folder_id",
                table: "event_schedule");

            migrationBuilder.AddColumn<int>(
                name: "schedule_id",
                table: "folder",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_folder_schedule_id",
                table: "folder",
                column: "schedule_id");

            migrationBuilder.AddForeignKey(
                name: "FK_folder_schedule_schedule_id",
                table: "folder",
                column: "schedule_id",
                principalTable: "schedule",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostDown(migrationBuilder);
        }
    }
}
