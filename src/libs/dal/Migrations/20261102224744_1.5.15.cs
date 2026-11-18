using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1515 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);

            migrationBuilder.AddColumn<bool>(
                name: "supports_function_calling",
                table: "llm",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);

            migrationBuilder.DropColumn(
                name: "supports_function_calling",
                table: "llm");

            PostDown(migrationBuilder);
        }
    }
}
