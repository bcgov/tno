using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1055 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.RenameIndex(
                name: "IX_name11",
                table: "topic",
                newName: "IX_topic_name");

            migrationBuilder.RenameIndex(
                name: "IX_name10",
                table: "tone_pool",
                newName: "IX_tone_pool_name");

            migrationBuilder.RenameIndex(
                name: "IX_name9",
                table: "tag",
                newName: "IX_tag_name");

            migrationBuilder.RenameIndex(
                name: "IX_code1",
                table: "tag",
                newName: "IX_tag_code");

            migrationBuilder.RenameIndex(
                name: "IX_name8",
                table: "source",
                newName: "IX_source_name");

            migrationBuilder.RenameIndex(
                name: "IX_code",
                table: "source",
                newName: "IX_source_code");

            migrationBuilder.CreateTable(
                name: "setting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValueSql: "''"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_setting", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_setting_is_enabled",
                table: "setting",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_setting_name",
                table: "setting",
                column: "name",
                unique: true);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropTable(
                name: "setting");

            migrationBuilder.RenameIndex(
                name: "IX_topic_name",
                table: "topic",
                newName: "IX_name11");

            migrationBuilder.RenameIndex(
                name: "IX_tone_pool_name",
                table: "tone_pool",
                newName: "IX_name10");

            migrationBuilder.RenameIndex(
                name: "IX_tag_name",
                table: "tag",
                newName: "IX_name9");

            migrationBuilder.RenameIndex(
                name: "IX_tag_code",
                table: "tag",
                newName: "IX_code1");

            migrationBuilder.RenameIndex(
                name: "IX_source_name",
                table: "source",
                newName: "IX_name8");

            migrationBuilder.RenameIndex(
                name: "IX_source_code",
                table: "source",
                newName: "IX_code");
            PostDown(migrationBuilder);
        }
    }
}
