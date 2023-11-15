using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1091 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AlterColumn<string>(
                name: "value",
                table: "setting",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "''");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.AlterColumn<string>(
                name: "value",
                table: "setting",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldDefaultValueSql: "''");
            PostDown(migrationBuilder);
        }
    }
}
