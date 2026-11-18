using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _10173 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.RenameColumn(
                name: "Format",
                table: "user_report",
                newName: "format2");

            migrationBuilder.AddColumn<int>(
                name: "send_to",
                table: "user_report",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "send_to",
                table: "user_av_overview",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "send_to",
                table: "user_report");

            migrationBuilder.DropColumn(
                name: "send_to",
                table: "user_av_overview");

            migrationBuilder.RenameColumn(
                name: "format2",
                table: "user_report",
                newName: "Format");
            PostDown(migrationBuilder);
        }
    }
}
