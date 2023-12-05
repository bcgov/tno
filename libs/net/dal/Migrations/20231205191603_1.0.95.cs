using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1095 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "media_type_search_group_id",
                table: "source");

            migrationBuilder.CreateTable(
                name: "source_media_type_search_mapping",
                columns: table => new
                {
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_media_type_search_mapping", x => new { x.source_id, x.media_type_id });
                    table.ForeignKey(
                        name: "FK_source_media_type_search_mapping_media_type_media_type_id",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_source_media_type_search_mapping_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_source_media_type_search_mapping_media_type_id",
                table: "source_media_type_search_mapping",
                column: "media_type_id");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropTable(
                name: "source_media_type_search_mapping");

            migrationBuilder.AddColumn<int>(
                name: "media_type_search_group_id",
                table: "source",
                type: "integer",
                nullable: true);
            PostDown(migrationBuilder);
        }
    }
}
