using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1010 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AlterColumn<string>(
                name: "page_min",
                table: "topic_score_rule",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "page_max",
                table: "topic_score_rule",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.AlterColumn<int>(
                name: "page_min",
                table: "topic_score_rule",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "page_max",
                table: "topic_score_rule",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true);
            PostDown(migrationBuilder);
        }
    }
}
