using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _105 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "tag",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "tag",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "tag_id",
                table: "content_tag",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(6)");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "code",
                table: "tag");

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "tag",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "tag_id",
                table: "content_tag",
                type: "character varying(6)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
            PostDown(migrationBuilder);
        }
    }
}
