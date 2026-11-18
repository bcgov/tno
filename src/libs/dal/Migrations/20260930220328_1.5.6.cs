using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _156 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);

            migrationBuilder.AddColumn<bool>(
                name: "is_system",
                table: "topic",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "topic_default_score",
                table: "source",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "analysis_id",
                table: "quote",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "owner",
                table: "quote",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "source_length",
                table: "quote",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source_start",
                table: "quote",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "context_window",
                table: "llm",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_output_tokens",
                table: "llm",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "requests_per_minute",
                table: "llm",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "token_estimation",
                table: "llm",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tokens_per_minute",
                table: "llm",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_score_overridden",
                table: "content_topic",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "score_rule_id",
                table: "content_topic",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "projection_revision",
                table: "content",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "analysis_backfill",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    start_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    time_zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_field = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    high_water_mark = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    checkpoint_content_id = table.Column<long>(type: "bigint", nullable: false),
                    total = table.Column<int>(type: "integer", nullable: false),
                    scheduled = table.Column<int>(type: "integer", nullable: false),
                    already_current = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_backfill", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analysis_topic",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    aliases = table.Column<string[]>(type: "text[]", nullable: false),
                    topic_id = table.Column<int>(type: "integer", nullable: true),
                    registry_version = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_topic", x => x.id);
                    table.ForeignKey(
                        name: "FK_analysis_topic_topic_topic_id",
                        column: x => x.topic_id,
                        principalTable: "topic",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "report_ai_result",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    report_id = table.Column<int>(type: "integer", nullable: false),
                    report_instance_id = table.Column<long>(type: "bigint", nullable: true),
                    report_section_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    manifest = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    output = table.Column<string>(type: "text", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    pipeline_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    request_count = table.Column<int>(type: "integer", nullable: false),
                    prompt_tokens = table.Column<int>(type: "integer", nullable: false),
                    completion_tokens = table.Column<int>(type: "integer", nullable: false),
                    duration_ms = table.Column<long>(type: "bigint", nullable: false),
                    story_count = table.Column<int>(type: "integer", nullable: false),
                    reduction_depth = table.Column<int>(type: "integer", nullable: false),
                    claim_expires_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_ai_result", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_ai_result_report_instance_report_instance_id",
                        column: x => x.report_instance_id,
                        principalTable: "report_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_ai_result_report_report_id",
                        column: x => x.report_id,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_ai_result_report_section_report_section_id",
                        column: x => x.report_section_id,
                        principalTable: "report_section",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "topic_rescore_job",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<int>(type: "integer", nullable: false),
                    start_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source_ids = table.Column<int[]>(type: "integer[]", nullable: false),
                    total = table.Column<int>(type: "integer", nullable: false),
                    processed = table.Column<int>(type: "integer", nullable: false),
                    changed = table.Column<int>(type: "integer", nullable: false),
                    failed = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    started_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic_rescore_job", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analysis_job",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    input_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    backfill_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    due_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    next_attempt_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lease_expires_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fencing_token = table.Column<long>(type: "bigint", nullable: false),
                    claimed_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_job", x => x.id);
                    table.ForeignKey(
                        name: "FK_analysis_job_analysis_backfill_backfill_id",
                        column: x => x.backfill_id,
                        principalTable: "analysis_backfill",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_analysis_job_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_analysis",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    input_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    is_metadata_only = table.Column<bool>(type: "boolean", nullable: false),
                    normalization_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    schema_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    prompt_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    llm_id = table.Column<int>(type: "integer", nullable: true),
                    model = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    summary = table.Column<string>(type: "text", nullable: false),
                    key_facts = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    entities = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    places = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    topics = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    primary_topic = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    analysis_topic_id = table.Column<int>(type: "integer", nullable: true),
                    suggested_tags = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    suggested_contributor = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    events = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    quotes = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    validation = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    prompt_tokens = table.Column<int>(type: "integer", nullable: false),
                    completion_tokens = table.Column<int>(type: "integer", nullable: false),
                    analyzed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_analysis", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_analysis_analysis_topic_analysis_topic_id",
                        column: x => x.analysis_topic_id,
                        principalTable: "analysis_topic",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_content_analysis_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_analysis_llm_llm_id",
                        column: x => x.llm_id,
                        principalTable: "llm",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "content_field_ownership",
                columns: table => new
                {
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    field = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner = table.Column<int>(type: "integer", nullable: false),
                    is_cleared = table.Column<bool>(type: "boolean", nullable: false),
                    analysis_id = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_field_ownership", x => new { x.content_id, x.field, x.value_key });
                    table.ForeignKey(
                        name: "FK_content_field_ownership_content_analysis_analysis_id",
                        column: x => x.analysis_id,
                        principalTable: "content_analysis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_content_field_ownership_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_created_on",
                table: "report_instance",
                column: "created_on");

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_sent_on",
                table: "report_instance",
                column: "sent_on");

            migrationBuilder.CreateIndex(
                name: "IX_quote_analysis_id",
                table: "quote",
                column: "analysis_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_instance_sent_on",
                table: "notification_instance",
                column: "sent_on");

            migrationBuilder.CreateIndex(
                name: "IX_content_topic_score_rule_id",
                table: "content_topic",
                column: "score_rule_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_content_topic_score",
                table: "content_topic",
                sql: "\"score\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_backfill_status",
                table: "analysis_backfill",
                columns: new[] { "status", "created_on" });

            migrationBuilder.CreateIndex(
                name: "IX_analysis_job_backfill_id",
                table: "analysis_job",
                column: "backfill_id");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_job_content_id",
                table: "analysis_job",
                column: "content_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_analysis_job_queue",
                table: "analysis_job",
                columns: new[] { "status", "priority", "due_on" });

            migrationBuilder.CreateIndex(
                name: "IX_analysis_topic_key",
                table: "analysis_topic",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_analysis_topic_topic_id",
                table: "analysis_topic",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_analysis_analysis_topic_id",
                table: "content_analysis",
                column: "analysis_topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_analysis_current",
                table: "content_analysis",
                columns: new[] { "content_id", "is_current" });

            migrationBuilder.CreateIndex(
                name: "IX_content_analysis_input",
                table: "content_analysis",
                columns: new[] { "content_id", "input_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_analysis_llm_id",
                table: "content_analysis",
                column: "llm_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_field_ownership_analysis_id",
                table: "content_field_ownership",
                column: "analysis_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_result_created_on",
                table: "report_ai_result",
                column: "created_on");

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_result_hash",
                table: "report_ai_result",
                column: "hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_result_report_id",
                table: "report_ai_result",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_result_report_instance_id",
                table: "report_ai_result",
                column: "report_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_result_report_section_id",
                table: "report_ai_result",
                column: "report_section_id");

            migrationBuilder.CreateIndex(
                name: "IX_topic_rescore_job_status",
                table: "topic_rescore_job",
                columns: new[] { "status", "created_on" });

            migrationBuilder.AddForeignKey(
                name: "FK_content_topic_topic_score_rule_score_rule_id",
                table: "content_topic",
                column: "score_rule_id",
                principalTable: "topic_score_rule",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_quote_content_analysis_analysis_id",
                table: "quote",
                column: "analysis_id",
                principalTable: "content_analysis",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "FK_content_topic_topic_score_rule_score_rule_id",
                table: "content_topic");

            migrationBuilder.DropForeignKey(
                name: "FK_quote_content_analysis_analysis_id",
                table: "quote");

            migrationBuilder.DropTable(
                name: "analysis_job");

            migrationBuilder.DropTable(
                name: "content_field_ownership");

            migrationBuilder.DropTable(
                name: "report_ai_result");

            migrationBuilder.DropTable(
                name: "topic_rescore_job");

            migrationBuilder.DropTable(
                name: "analysis_backfill");

            migrationBuilder.DropTable(
                name: "content_analysis");

            migrationBuilder.DropTable(
                name: "analysis_topic");

            migrationBuilder.DropIndex(
                name: "IX_report_instance_created_on",
                table: "report_instance");

            migrationBuilder.DropIndex(
                name: "IX_report_instance_sent_on",
                table: "report_instance");

            migrationBuilder.DropIndex(
                name: "IX_quote_analysis_id",
                table: "quote");

            migrationBuilder.DropIndex(
                name: "IX_notification_instance_sent_on",
                table: "notification_instance");

            migrationBuilder.DropIndex(
                name: "IX_content_topic_score_rule_id",
                table: "content_topic");

            migrationBuilder.DropCheckConstraint(
                name: "CK_content_topic_score",
                table: "content_topic");

            migrationBuilder.DropColumn(
                name: "is_system",
                table: "topic");

            migrationBuilder.DropColumn(
                name: "topic_default_score",
                table: "source");

            migrationBuilder.DropColumn(
                name: "analysis_id",
                table: "quote");

            migrationBuilder.DropColumn(
                name: "owner",
                table: "quote");

            migrationBuilder.DropColumn(
                name: "source_length",
                table: "quote");

            migrationBuilder.DropColumn(
                name: "source_start",
                table: "quote");

            migrationBuilder.DropColumn(
                name: "context_window",
                table: "llm");

            migrationBuilder.DropColumn(
                name: "max_output_tokens",
                table: "llm");

            migrationBuilder.DropColumn(
                name: "requests_per_minute",
                table: "llm");

            migrationBuilder.DropColumn(
                name: "token_estimation",
                table: "llm");

            migrationBuilder.DropColumn(
                name: "tokens_per_minute",
                table: "llm");

            migrationBuilder.DropColumn(
                name: "is_score_overridden",
                table: "content_topic");

            migrationBuilder.DropColumn(
                name: "score_rule_id",
                table: "content_topic");

            migrationBuilder.DropColumn(
                name: "projection_revision",
                table: "content");

            PostDown(migrationBuilder);
        }
    }
}
