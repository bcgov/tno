using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _157 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);

            migrationBuilder.DropTable(
                name: "analysis_job");

            migrationBuilder.DropTable(
                name: "topic_rescore_job");

            migrationBuilder.DropTable(
                name: "analysis_backfill");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "metadata",
                table: "content",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);

            migrationBuilder.DropColumn(
                name: "metadata",
                table: "content");

            migrationBuilder.CreateTable(
                name: "analysis_backfill",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    already_current = table.Column<int>(type: "integer", nullable: false),
                    checkpoint_content_id = table.Column<long>(type: "bigint", nullable: false),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    date_field = table.Column<int>(type: "integer", nullable: false),
                    end_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    high_water_mark = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    scheduled = table.Column<int>(type: "integer", nullable: false),
                    start_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    time_zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    total = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_backfill", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "topic_rescore_job",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    changed = table.Column<int>(type: "integer", nullable: false),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    end_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    failed = table.Column<int>(type: "integer", nullable: false),
                    processed = table.Column<int>(type: "integer", nullable: false),
                    source_ids = table.Column<int[]>(type: "integer[]", nullable: false),
                    start_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total = table.Column<int>(type: "integer", nullable: false),
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
                    backfill_id = table.Column<long>(type: "bigint", nullable: true),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    claimed_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    due_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fencing_token = table.Column<long>(type: "bigint", nullable: false),
                    input_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    lease_expires_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_attempt_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                name: "IX_topic_rescore_job_status",
                table: "topic_rescore_job",
                columns: new[] { "status", "created_on" });

            PostDown(migrationBuilder);
        }
    }
}
