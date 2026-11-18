using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1036 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "template",
                table: "report");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled14",
                table: "topic",
                newName: "IX_topic_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled13",
                table: "tone_pool",
                newName: "IX_tonepool_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled12",
                table: "tag",
                newName: "IX_tag_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled11",
                table: "source",
                newName: "IX_source_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled10",
                table: "series",
                newName: "IX_series_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled9",
                table: "report",
                newName: "IX_report_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled8",
                table: "product",
                newName: "IX_product_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled7",
                table: "notification",
                newName: "IX_notification_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled6",
                table: "metric",
                newName: "IX_metric_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled5",
                table: "license",
                newName: "IX_license_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled4",
                table: "ingest_type",
                newName: "IX_ingesttype_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled3",
                table: "data_location",
                newName: "IX_datalocation_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled2",
                table: "contributor",
                newName: "IX_contributor_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled1",
                table: "connection",
                newName: "IX_connection_is_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled",
                table: "action",
                newName: "IX_action_is_enabled");

            migrationBuilder.AddColumn<int>(
                name: "owner_id",
                table: "report_instance",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "report_template_id",
                table: "report",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "chart_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    template = table.Column<string>(type: "text", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_chart_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "earned_media",
                columns: table => new
                {
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    content_type = table.Column<int>(type: "integer", nullable: false),
                    length_of_content = table.Column<int>(type: "integer", nullable: false),
                    rate = table.Column<float>(type: "real", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_earned_media", x => new { x.source_id, x.content_type });
                    table.ForeignKey(
                        name: "FK_earned_media_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    enable_sections = table.Column<bool>(type: "boolean", nullable: false),
                    enable_section_summary = table.Column<bool>(type: "boolean", nullable: false),
                    enable_summary = table.Column<bool>(type: "boolean", nullable: false),
                    enable_charts = table.Column<bool>(type: "boolean", nullable: false),
                    enable_charts_over_time = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_report_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sentiment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<float>(type: "real", nullable: false),
                    rate = table.Column<float>(type: "real", nullable: false),
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
                    table.PrimaryKey("PK_sentiment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_template_chart_template",
                columns: table => new
                {
                    report_template_id = table.Column<int>(type: "integer", nullable: false),
                    chart_template_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_template_chart_template", x => new { x.report_template_id, x.chart_template_id });
                    table.ForeignKey(
                        name: "FK_report_template_chart_template_chart_template_chart_templat~",
                        column: x => x.chart_template_id,
                        principalTable: "chart_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_template_chart_template_report_template_report_templ~",
                        column: x => x.report_template_id,
                        principalTable: "report_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_owner_id",
                table: "report_instance",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_report_template_id",
                table: "report",
                column: "report_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_chart_template_name",
                table: "chart_template",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_charttemplate_is_enabled",
                table: "chart_template",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_report_template_name",
                table: "report_template",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reporttemplate_is_enabled",
                table: "report_template",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_report_template_chart_template_chart_template_id",
                table: "report_template_chart_template",
                column: "chart_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_sentiment_is_enabled",
                table: "sentiment",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_sentiment_name",
                table: "sentiment",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_report_report_template_report_template_id",
                table: "report",
                column: "report_template_id",
                principalTable: "report_template",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_report_instance_user_owner_id",
                table: "report_instance",
                column: "owner_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_report_report_template_report_template_id",
                table: "report");

            migrationBuilder.DropForeignKey(
                name: "FK_report_instance_user_owner_id",
                table: "report_instance");

            migrationBuilder.DropTable(
                name: "earned_media");

            migrationBuilder.DropTable(
                name: "report_template_chart_template");

            migrationBuilder.DropTable(
                name: "sentiment");

            migrationBuilder.DropTable(
                name: "chart_template");

            migrationBuilder.DropTable(
                name: "report_template");

            migrationBuilder.DropIndex(
                name: "IX_report_instance_owner_id",
                table: "report_instance");

            migrationBuilder.DropIndex(
                name: "IX_report_report_template_id",
                table: "report");

            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "report_instance");

            migrationBuilder.DropColumn(
                name: "report_template_id",
                table: "report");

            migrationBuilder.RenameIndex(
                name: "IX_topic_is_enabled",
                table: "topic",
                newName: "IX_is_enabled14");

            migrationBuilder.RenameIndex(
                name: "IX_tonepool_is_enabled",
                table: "tone_pool",
                newName: "IX_is_enabled13");

            migrationBuilder.RenameIndex(
                name: "IX_tag_is_enabled",
                table: "tag",
                newName: "IX_is_enabled12");

            migrationBuilder.RenameIndex(
                name: "IX_source_is_enabled",
                table: "source",
                newName: "IX_is_enabled11");

            migrationBuilder.RenameIndex(
                name: "IX_series_is_enabled",
                table: "series",
                newName: "IX_is_enabled10");

            migrationBuilder.RenameIndex(
                name: "IX_report_is_enabled",
                table: "report",
                newName: "IX_is_enabled9");

            migrationBuilder.RenameIndex(
                name: "IX_product_is_enabled",
                table: "product",
                newName: "IX_is_enabled8");

            migrationBuilder.RenameIndex(
                name: "IX_notification_is_enabled",
                table: "notification",
                newName: "IX_is_enabled7");

            migrationBuilder.RenameIndex(
                name: "IX_metric_is_enabled",
                table: "metric",
                newName: "IX_is_enabled6");

            migrationBuilder.RenameIndex(
                name: "IX_license_is_enabled",
                table: "license",
                newName: "IX_is_enabled5");

            migrationBuilder.RenameIndex(
                name: "IX_ingesttype_is_enabled",
                table: "ingest_type",
                newName: "IX_is_enabled4");

            migrationBuilder.RenameIndex(
                name: "IX_datalocation_is_enabled",
                table: "data_location",
                newName: "IX_is_enabled3");

            migrationBuilder.RenameIndex(
                name: "IX_contributor_is_enabled",
                table: "contributor",
                newName: "IX_is_enabled2");

            migrationBuilder.RenameIndex(
                name: "IX_connection_is_enabled",
                table: "connection",
                newName: "IX_is_enabled1");

            migrationBuilder.RenameIndex(
                name: "IX_action_is_enabled",
                table: "action",
                newName: "IX_is_enabled");

            migrationBuilder.AddColumn<string>(
                name: "template",
                table: "report",
                type: "text",
                nullable: false,
                defaultValue: "");
            PostDown(migrationBuilder);
        }
    }
}
