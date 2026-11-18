using System;
using TNO.DAL;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1047 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "enable_charts",
                table: "report_template");

            migrationBuilder.DropColumn(
                name: "enable_charts_over_time",
                table: "report_template");

            migrationBuilder.DropColumn(
                name: "enable_section_summary",
                table: "report_template");

            migrationBuilder.DropColumn(
                name: "enable_sections",
                table: "report_template");

            migrationBuilder.DropColumn(
                name: "enable_summary",
                table: "report_template");

            migrationBuilder.DropColumn(
                name: "filter",
                table: "report");

            migrationBuilder.DropColumn(
                name: "report_type",
                table: "report");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "settings",
                table: "report_template",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "report",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "aliases",
                table: "minister",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true,
                oldDefaultValueSql: "''");

            migrationBuilder.AddColumn<int>(
                name: "organization_id",
                table: "minister",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "position",
                table: "minister",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "settings",
                table: "chart_template",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.CreateTable(
                name: "filter",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    query = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
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
                    table.PrimaryKey("PK_filter", x => x.id);
                    table.ForeignKey(
                        name: "FK_filter_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "folder",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
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
                    table.PrimaryKey("PK_folder", x => x.id);
                    table.ForeignKey(
                        name: "FK_folder_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_organization", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_organization_parent_id",
                        column: x => x.parent_id,
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "folder_content",
                columns: table => new
                {
                    folder_id = table.Column<int>(type: "integer", nullable: false),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_folder_content", x => new { x.folder_id, x.content_id });
                    table.ForeignKey(
                        name: "FK_folder_content_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_folder_content_folder_folder_id",
                        column: x => x.folder_id,
                        principalTable: "folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_section",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_id = table.Column<int>(type: "integer", nullable: false),
                    filter_id = table.Column<int>(type: "integer", nullable: true),
                    folder_id = table.Column<int>(type: "integer", nullable: true),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
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
                    table.PrimaryKey("PK_report_section", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_section_filter_filter_id",
                        column: x => x.filter_id,
                        principalTable: "filter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_section_folder_folder_id",
                        column: x => x.folder_id,
                        principalTable: "folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_section_report_report_id",
                        column: x => x.report_id,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_organization",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    organization_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_organization", x => new { x.user_id, x.organization_id });
                    table.ForeignKey(
                        name: "FK_user_organization_organization_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_organization_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_section_chart_template",
                columns: table => new
                {
                    report_section_id = table.Column<int>(type: "integer", nullable: false),
                    chart_template_id = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_section_chart_template", x => new { x.report_section_id, x.chart_template_id });
                    table.ForeignKey(
                        name: "FK_report_section_chart_template_chart_template_chart_template~",
                        column: x => x.chart_template_id,
                        principalTable: "chart_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_section_chart_template_report_section_report_section~",
                        column: x => x.report_section_id,
                        principalTable: "report_section",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_minister_name",
                table: "minister",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_minister_organization_id",
                table: "minister",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_filter_is_enabled",
                table: "filter",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_filter_owner_id_name",
                table: "filter",
                columns: new[] { "owner_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_folder_is_enabled",
                table: "folder",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_folder_owner_id_name",
                table: "folder",
                columns: new[] { "owner_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_folder_content_content_id",
                table: "folder_content",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_is_enabled",
                table: "organization",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_organization_parent_id_name",
                table: "organization",
                columns: new[] { "parent_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_section_filter_id",
                table: "report_section",
                column: "filter_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_section_folder_id",
                table: "report_section",
                column: "folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_section_report_id_name",
                table: "report_section",
                columns: new[] { "report_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reportsection_is_enabled",
                table: "report_section",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_report_section_chart_template_chart_template_id",
                table: "report_section_chart_template",
                column: "chart_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_organization_organization_id",
                table: "user_organization",
                column: "organization_id");

            migrationBuilder.AddForeignKey(
                name: "FK_minister_organization_organization_id",
                table: "minister",
                column: "organization_id",
                principalTable: "organization",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_minister_organization_organization_id",
                table: "minister");

            migrationBuilder.DropTable(
                name: "folder_content");

            migrationBuilder.DropTable(
                name: "report_section_chart_template");

            migrationBuilder.DropTable(
                name: "user_organization");

            migrationBuilder.DropTable(
                name: "report_section");

            migrationBuilder.DropTable(
                name: "organization");

            migrationBuilder.DropTable(
                name: "filter");

            migrationBuilder.DropTable(
                name: "folder");

            migrationBuilder.DropIndex(
                name: "IX_minister_name",
                table: "minister");

            migrationBuilder.DropIndex(
                name: "IX_minister_organization_id",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "settings",
                table: "report_template");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "position",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "settings",
                table: "chart_template");

            migrationBuilder.AddColumn<bool>(
                name: "enable_charts",
                table: "report_template",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "enable_charts_over_time",
                table: "report_template",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "enable_section_summary",
                table: "report_template",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "enable_sections",
                table: "report_template",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "enable_summary",
                table: "report_template",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "report",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "filter",
                table: "report",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.AddColumn<int>(
                name: "report_type",
                table: "report",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "aliases",
                table: "minister",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                defaultValueSql: "''",
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldDefaultValueSql: "''");
            PostDown(migrationBuilder);
        }
    }
}
