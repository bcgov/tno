using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _104 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<int>(
                name: "requested_by_id",
                table: "schedule",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "run_only_once",
                table: "schedule",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_requested_by_id",
                table: "schedule",
                column: "requested_by_id");

            migrationBuilder.AddForeignKey(
                name: "FK_schedule_user_requested_by_id",
                table: "schedule",
                column: "requested_by_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_schedule_user_requested_by_id",
                table: "schedule");

            migrationBuilder.DropIndex(
                name: "IX_schedule_requested_by_id",
                table: "schedule");

            migrationBuilder.DropColumn(
                name: "requested_by_id",
                table: "schedule");

            migrationBuilder.DropColumn(
                name: "run_only_once",
                table: "schedule");
            PostDown(migrationBuilder);
        }
    }
}
