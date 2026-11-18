using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1027 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropPrimaryKey(
                name: "PK_report_instance_content",
                table: "report_instance_content");

            migrationBuilder.AddColumn<string>(
                name: "SectionName",
                table: "report_instance_content",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddPrimaryKey(
                name: "PK_report_instance_content",
                table: "report_instance_content",
                columns: new[] { "report_instance_id", "content_id", "SectionName" });
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropPrimaryKey(
                name: "PK_report_instance_content",
                table: "report_instance_content");

            migrationBuilder.DropColumn(
                name: "SectionName",
                table: "report_instance_content");

            migrationBuilder.AddPrimaryKey(
                name: "PK_report_instance_content",
                table: "report_instance_content",
                columns: new[] { "report_instance_id", "content_id" });
            PostDown(migrationBuilder);
        }
    }
}
