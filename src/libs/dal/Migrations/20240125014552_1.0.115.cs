using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _10115 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<int>(
                name: "linked_report_id",
                table: "report_section",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_section_linked_report_id",
                table: "report_section",
                column: "linked_report_id");

            migrationBuilder.AddForeignKey(
                name: "FK_report_section_report_linked_report_id",
                table: "report_section",
                column: "linked_report_id",
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
                name: "FK_report_section_report_linked_report_id",
                table: "report_section");

            migrationBuilder.DropIndex(
                name: "IX_report_section_linked_report_id",
                table: "report_section");

            migrationBuilder.DropColumn(
                name: "linked_report_id",
                table: "report_section");
            PostDown(migrationBuilder);
        }
    }
}
