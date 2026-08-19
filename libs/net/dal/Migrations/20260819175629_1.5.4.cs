using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _154 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<JsonDocument>(
                name: "compare_definition",
                table: "automation_run",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_dry_run",
                table: "automation_run",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "definition",
                table: "automation_profile",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "automation_run_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    automation_run_id = table.Column<long>(type: "bigint", nullable: false),
                    step_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    action_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    action_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    analysis_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    content_id = table.Column<long>(type: "bigint", nullable: true),
                    attempt = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_llm = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    variant = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    prompt = table.Column<string>(type: "text", nullable: true),
                    response = table.Column<string>(type: "text", nullable: true),
                    prompt_tokens = table.Column<int>(type: "integer", nullable: true),
                    completion_tokens = table.Column<int>(type: "integer", nullable: true),
                    duration_ms = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    outcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValueSql: "''"),
                    detail = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_run_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_run_log_automation_run_automation_run_id",
                        column: x => x.automation_run_id,
                        principalTable: "automation_run",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_automation_run_log_content",
                table: "automation_run_log",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_run_log_created",
                table: "automation_run_log",
                column: "created_on");

            migrationBuilder.CreateIndex(
                name: "IX_automation_run_log_run",
                table: "automation_run_log",
                column: "automation_run_id");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropTable(
                name: "automation_run_log");

            migrationBuilder.DropColumn(
                name: "compare_definition",
                table: "automation_run");

            migrationBuilder.DropColumn(
                name: "is_dry_run",
                table: "automation_run");

            migrationBuilder.DropColumn(
                name: "definition",
                table: "automation_profile");
            PostDown(migrationBuilder);
        }
    }
}
