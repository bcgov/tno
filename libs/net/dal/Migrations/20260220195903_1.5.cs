using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _15 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<string>(
                name: "link_body",
                table: "report_instance",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "body",
                table: "av_overview_instance",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "av_overview_instance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "subject",
                table: "av_overview_instance",
                type: "text",
                nullable: false,
                defaultValue: "");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "link_body",
                table: "report_instance");

            migrationBuilder.DropColumn(
                name: "body",
                table: "av_overview_instance");

            migrationBuilder.DropColumn(
                name: "status",
                table: "av_overview_instance");

            migrationBuilder.DropColumn(
                name: "subject",
                table: "av_overview_instance");
            PostDown(migrationBuilder);
        }
    }
}
