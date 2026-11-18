using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1073 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<int>(
                name: "filter_id",
                table: "folder",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "schedule_id",
                table: "folder",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_folder_filter_id",
                table: "folder",
                column: "filter_id");

            migrationBuilder.CreateIndex(
                name: "IX_folder_schedule_id",
                table: "folder",
                column: "schedule_id");

            migrationBuilder.AddForeignKey(
                name: "FK_folder_filter_filter_id",
                table: "folder",
                column: "filter_id",
                principalTable: "filter",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_folder_schedule_schedule_id",
                table: "folder",
                column: "schedule_id",
                principalTable: "schedule",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_folder_filter_filter_id",
                table: "folder");

            migrationBuilder.DropForeignKey(
                name: "FK_folder_schedule_schedule_id",
                table: "folder");

            migrationBuilder.DropIndex(
                name: "IX_folder_filter_id",
                table: "folder");

            migrationBuilder.DropIndex(
                name: "IX_folder_schedule_id",
                table: "folder");

            migrationBuilder.DropColumn(
                name: "filter_id",
                table: "folder");

            migrationBuilder.DropColumn(
                name: "schedule_id",
                table: "folder");
            PostDown(migrationBuilder);
        }
    }
}
