using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1016 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.RenameIndex(
                name: "IX_is_enabled13",
                table: "topic",
                newName: "IX_is_enabled14");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled12",
                table: "tone_pool",
                newName: "IX_is_enabled13");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled11",
                table: "tag",
                newName: "IX_is_enabled12");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled10",
                table: "source",
                newName: "IX_is_enabled11");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled9",
                table: "series",
                newName: "IX_is_enabled10");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled8",
                table: "report",
                newName: "IX_is_enabled9");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled7",
                table: "product",
                newName: "IX_is_enabled8");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled6",
                table: "notification",
                newName: "IX_is_enabled7");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled5",
                table: "metric",
                newName: "IX_is_enabled6");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled4",
                table: "license",
                newName: "IX_is_enabled5");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled3",
                table: "ingest_type",
                newName: "IX_is_enabled4");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled2",
                table: "data_location",
                newName: "IX_is_enabled3");

            migrationBuilder.AddColumn<int>(
                name: "contributor_id",
                table: "content",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "contributor",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_id = table.Column<int>(type: "integer", nullable: true),
                    auto_transcribe = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_contributor", x => x.id);
                    table.ForeignKey(
                        name: "FK_contributor_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_content_contributor_id",
                table: "content",
                column: "contributor_id");

            migrationBuilder.CreateIndex(
                name: "IX_contributor_source_id",
                table: "contributor",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled2",
                table: "contributor",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.AddForeignKey(
                name: "FK_content_contributor_contributor_id",
                table: "content",
                column: "contributor_id",
                principalTable: "contributor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_content_contributor_contributor_id",
                table: "content");

            migrationBuilder.DropTable(
                name: "contributor");

            migrationBuilder.DropIndex(
                name: "IX_content_contributor_id",
                table: "content");

            migrationBuilder.DropColumn(
                name: "contributor_id",
                table: "content");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled14",
                table: "topic",
                newName: "IX_is_enabled13");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled13",
                table: "tone_pool",
                newName: "IX_is_enabled12");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled12",
                table: "tag",
                newName: "IX_is_enabled11");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled11",
                table: "source",
                newName: "IX_is_enabled10");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled10",
                table: "series",
                newName: "IX_is_enabled9");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled9",
                table: "report",
                newName: "IX_is_enabled8");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled8",
                table: "product",
                newName: "IX_is_enabled7");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled7",
                table: "notification",
                newName: "IX_is_enabled6");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled6",
                table: "metric",
                newName: "IX_is_enabled5");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled5",
                table: "license",
                newName: "IX_is_enabled4");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled4",
                table: "ingest_type",
                newName: "IX_is_enabled3");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled3",
                table: "data_location",
                newName: "IX_is_enabled2");
            PostDown(migrationBuilder);
        }
    }
}
