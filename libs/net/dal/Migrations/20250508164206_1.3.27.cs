using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1327 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media_analytics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    published_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    unique_views = table.Column<int>(type: "integer", nullable: false),
                    total_views = table.Column<int>(type: "integer", nullable: false),
                    average_views = table.Column<float>(type: "real", nullable: false),
                    male_viewers = table.Column<float>(type: "real", nullable: false),
                    age_group1 = table.Column<float>(type: "real", nullable: false),
                    age_group1_label = table.Column<string>(type: "text", nullable: false),
                    age_group2 = table.Column<float>(type: "real", nullable: false),
                    age_group2_label = table.Column<string>(type: "text", nullable: false),
                    age_group3 = table.Column<float>(type: "real", nullable: false),
                    age_group3_label = table.Column<string>(type: "text", nullable: false),
                    age_group4 = table.Column<float>(type: "real", nullable: false),
                    age_group4_label = table.Column<string>(type: "text", nullable: false),
                    page_views1 = table.Column<float>(type: "real", nullable: false),
                    page_views1_label = table.Column<string>(type: "text", nullable: false),
                    page_views2 = table.Column<float>(type: "real", nullable: false),
                    page_views2_label = table.Column<string>(type: "text", nullable: false),
                    page_views3 = table.Column<float>(type: "real", nullable: false),
                    page_views3_label = table.Column<string>(type: "text", nullable: false),
                    page_views4 = table.Column<float>(type: "real", nullable: false),
                    page_views4_label = table.Column<string>(type: "text", nullable: false),
                    watch_time1 = table.Column<float>(type: "real", nullable: false),
                    watch_time1_label = table.Column<string>(type: "text", nullable: false),
                    watch_time2 = table.Column<float>(type: "real", nullable: false),
                    watch_time2_label = table.Column<string>(type: "text", nullable: false),
                    watch_time3 = table.Column<float>(type: "real", nullable: false),
                    watch_time3_label = table.Column<string>(type: "text", nullable: false),
                    watch_time4 = table.Column<float>(type: "real", nullable: false),
                    watch_time4_label = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_media_analytics", x => x.id);
                    table.ForeignKey(
                        name: "FK_media_analytics_media_type_media_type_id",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_media_analytics_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_media_analytics_media_type_id",
                table: "media_analytics",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_media_analytics_published_on_source_id_media_type_id",
                table: "media_analytics",
                columns: new[] { "published_on", "source_id", "media_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_analytics_source_id",
                table: "media_analytics",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediaanalytics_is_enabled",
                table: "media_analytics",
                columns: new[] { "is_enabled", "name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_analytics");
        }
    }
}
