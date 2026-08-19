using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _153 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<int>(
                name: "filter_id",
                table: "automation_action",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_automation_action_filter_id",
                table: "automation_action",
                column: "filter_id");

            migrationBuilder.AddForeignKey(
                name: "FK_automation_action_filter_filter_id",
                table: "automation_action",
                column: "filter_id",
                principalTable: "filter",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_automation_action_filter_filter_id",
                table: "automation_action");

            migrationBuilder.DropIndex(
                name: "IX_automation_action_filter_id",
                table: "automation_action");

            migrationBuilder.DropColumn(
                name: "filter_id",
                table: "automation_action");
            PostDown(migrationBuilder);
        }
    }
}
