using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _152 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<int>(
                name: "automation_profile_id",
                table: "event_schedule",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "automation_profile",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    schema_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    filter_id = table.Column<int>(type: "integer", nullable: true),
                    llm_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_automation_profile", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_profile_filter_filter_id",
                        column: x => x.filter_id,
                        principalTable: "filter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_automation_profile_llm_llm_id",
                        column: x => x.llm_id,
                        principalTable: "llm",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "automation_run",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    automation_profile_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    trigger = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValueSql: "'manual'"),
                    note = table.Column<string>(type: "text", nullable: true),
                    started_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    summary = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_run", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_run_automation_profile_automation_profile_id",
                        column: x => x.automation_profile_id,
                        principalTable: "automation_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "automation_step",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    automation_profile_id = table.Column<int>(type: "integer", nullable: false),
                    prompt = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''"),
                    target = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'content'"),
                    filter_id = table.Column<int>(type: "integer", nullable: true),
                    llm_id = table.Column<int>(type: "integer", nullable: true),
                    send_separate_prompts = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    use_chat_completions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    apply_to_automation_filter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    iterate_step_filter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_automation_step", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_step_automation_profile_automation_profile_id",
                        column: x => x.automation_profile_id,
                        principalTable: "automation_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_automation_step_filter_filter_id",
                        column: x => x.filter_id,
                        principalTable: "filter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_automation_step_llm_llm_id",
                        column: x => x.llm_id,
                        principalTable: "llm",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "automation_run_response",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    automation_run_id = table.Column<long>(type: "bigint", nullable: false),
                    step_id = table.Column<int>(type: "integer", nullable: false),
                    step_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    action_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    content_id = table.Column<long>(type: "bigint", nullable: true),
                    prompt = table.Column<string>(type: "text", nullable: true),
                    response = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_run_response", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_run_response_automation_run_automation_run_id",
                        column: x => x.automation_run_id,
                        principalTable: "automation_run",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "automation_action",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    automation_step_id = table.Column<int>(type: "integer", nullable: false),
                    prompt = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''"),
                    action_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    max_calls = table.Column<int>(type: "integer", nullable: true),
                    confirmation_statement = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''"),
                    content_field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    content_action_id = table.Column<int>(type: "integer", nullable: true),
                    objective = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    report_id = table.Column<int>(type: "integer", nullable: true),
                    notification_id = table.Column<int>(type: "integer", nullable: true),
                    prior_action_id = table.Column<int>(type: "integer", nullable: true),
                    llm_id = table.Column<int>(type: "integer", nullable: true),
                    auto_execute = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    abort_if_no_confirmation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    works_on = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    create_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    create_clone = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_action", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_action_action_content_action_id",
                        column: x => x.content_action_id,
                        principalTable: "action",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_automation_action_automation_action_prior_action_id",
                        column: x => x.prior_action_id,
                        principalTable: "automation_action",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_automation_action_automation_step_automation_step_id",
                        column: x => x.automation_step_id,
                        principalTable: "automation_step",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_automation_action_llm_llm_id",
                        column: x => x.llm_id,
                        principalTable: "llm",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_automation_action_notification_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_automation_action_report_report_id",
                        column: x => x.report_id,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_schedule_automation_profile_id",
                table: "event_schedule",
                column: "automation_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_action_automation_step_id",
                table: "automation_action",
                column: "automation_step_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_action_content_action_id",
                table: "automation_action",
                column: "content_action_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_action_llm_id",
                table: "automation_action",
                column: "llm_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_action_notification_id",
                table: "automation_action",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_action_prior_action_id",
                table: "automation_action",
                column: "prior_action_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_action_report_id",
                table: "automation_action",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_profile_filter_id",
                table: "automation_profile",
                column: "filter_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_profile_llm_id",
                table: "automation_profile",
                column: "llm_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_profile_name",
                table: "automation_profile",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_automationprofile_is_enabled",
                table: "automation_profile",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_automation_run_profile_started",
                table: "automation_run",
                columns: new[] { "automation_profile_id", "started_on" });

            migrationBuilder.CreateIndex(
                name: "IX_automation_run_started",
                table: "automation_run",
                column: "started_on");

            migrationBuilder.CreateIndex(
                name: "IX_automation_run_response_run",
                table: "automation_run_response",
                column: "automation_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_step_automation_profile_id",
                table: "automation_step",
                column: "automation_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_step_filter_id",
                table: "automation_step",
                column: "filter_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_step_llm_id",
                table: "automation_step",
                column: "llm_id");

            migrationBuilder.CreateIndex(
                name: "IX_automationstep_is_enabled",
                table: "automation_step",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.AddForeignKey(
                name: "FK_event_schedule_automation_profile_automation_profile_id",
                table: "event_schedule",
                column: "automation_profile_id",
                principalTable: "automation_profile",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_event_schedule_automation_profile_automation_profile_id",
                table: "event_schedule");

            migrationBuilder.DropTable(
                name: "automation_action");

            migrationBuilder.DropTable(
                name: "automation_run_response");

            migrationBuilder.DropTable(
                name: "automation_step");

            migrationBuilder.DropTable(
                name: "automation_run");

            migrationBuilder.DropTable(
                name: "automation_profile");

            migrationBuilder.DropIndex(
                name: "IX_event_schedule_automation_profile_id",
                table: "event_schedule");

            migrationBuilder.DropColumn(
                name: "automation_profile_id",
                table: "event_schedule");
            PostDown(migrationBuilder);
        }
    }
}
