using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1321 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "report_section",
                type: "text",
                nullable: false,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldDefaultValueSql: "''");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "report_section",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "''");
            PostDown(migrationBuilder);
        }
    }
}
