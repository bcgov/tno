using System;
using TNO.DAL;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _106 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropTable(
                name: "content_category");

            migrationBuilder.DropTable(
                name: "source_source_action");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "source_action");

            migrationBuilder.DropIndex(
                name: "IX_name13",
                table: "tone_pool");

            migrationBuilder.DropIndex(
                name: "IX_tone_pool_owner_id",
                table: "tone_pool");

            migrationBuilder.CreateIndex(
                name: "IX_name11",
                table: "tag",
                columns: new[] { "name" });

            migrationBuilder.RenameIndex(
                name: "IX_name2",
                table: "connection",
                newName: "IX_name1");

            migrationBuilder.RenameIndex(
                name: "IX_name3",
                table: "data_location",
                newName: "IX_name2");

            migrationBuilder.RenameIndex(
                name: "IX_name4",
                table: "ingest",
                newName: "IX_name3");

            migrationBuilder.RenameIndex(
                name: "IX_name5",
                table: "ingest_type",
                newName: "IX_name4");

            migrationBuilder.RenameIndex(
                name: "IX_name6",
                table: "license",
                newName: "IX_name5");

            migrationBuilder.RenameIndex(
                name: "IX_name7",
                table: "metric",
                newName: "IX_name6");

            migrationBuilder.RenameIndex(
                name: "IX_name8",
                table: "product",
                newName: "IX_name7");

            migrationBuilder.AddColumn<bool>(
                name: "use_in_topics",
                table: "source",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "use_in_topics",
                table: "series",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "report",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_type = table.Column<int>(type: "integer", nullable: false),
                    filter = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
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
                    table.PrimaryKey("PK_report", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "topic",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    topic_type = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_topic", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "topic_score_rule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    section = table.Column<string>(type: "text", nullable: true),
                    page_min = table.Column<int>(type: "integer", nullable: true),
                    page_max = table.Column<int>(type: "integer", nullable: true),
                    has_image = table.Column<bool>(type: "boolean", nullable: true),
                    time_min = table.Column<TimeSpan>(type: "interval", nullable: true),
                    time_max = table.Column<TimeSpan>(type: "interval", nullable: true),
                    char_min = table.Column<int>(type: "integer", nullable: true),
                    char_max = table.Column<int>(type: "integer", nullable: true),
                    score = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic_score_rule", x => x.id);
                    table.ForeignKey(
                        name: "FK_topic_score_rule_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_instance",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportId = table.Column<int>(type: "integer", nullable: false),
                    PublishedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_instance", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_instance_report_ReportId",
                        column: x => x.ReportId,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_topic",
                columns: table => new
                {
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    topic_id = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_topic", x => new { x.content_id, x.topic_id });
                    table.ForeignKey(
                        name: "FK_content_topic_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_topic_topic_topic_id",
                        column: x => x.topic_id,
                        principalTable: "topic",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_instance_content",
                columns: table => new
                {
                    report_instance_id = table.Column<long>(type: "bigint", nullable: false),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_instance_content", x => new { x.report_instance_id, x.content_id });
                    table.ForeignKey(
                        name: "FK_report_instance_content_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_instance_content_report_instance_report_instance_id",
                        column: x => x.report_instance_id,
                        principalTable: "report_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled11",
                table: "tone_pool",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name12",
                table: "tone_pool",
                columns: new[] { "owner_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled10",
                table: "tag",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled9",
                table: "source",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled8",
                table: "series",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled6",
                table: "product",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled5",
                table: "metric",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled4",
                table: "license",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled3",
                table: "ingest_type",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled2",
                table: "data_location",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled1",
                table: "connection",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled",
                table: "action",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_content_topic_topic_id",
                table: "content_topic",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled7",
                table: "report",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name8",
                table: "report",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_dates",
                table: "report_instance",
                columns: new[] { "PublishedOn", "created_on" });

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_ReportId",
                table: "report_instance",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_content_content_id",
                table: "report_instance_content",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled12",
                table: "topic",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name13",
                table: "topic",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_topic_score_rule_source_id",
                table: "topic_score_rule",
                column: "source_id");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropTable(
                name: "content_topic");

            migrationBuilder.DropTable(
                name: "report_instance_content");

            migrationBuilder.DropTable(
                name: "topic_score_rule");

            migrationBuilder.DropTable(
                name: "topic");

            migrationBuilder.DropTable(
                name: "report_instance");

            migrationBuilder.DropTable(
                name: "report");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled11",
                table: "tone_pool");

            migrationBuilder.DropIndex(
                name: "IX_name12",
                table: "tone_pool");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled10",
                table: "tag");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled9",
                table: "source");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled8",
                table: "series");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled6",
                table: "product");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled5",
                table: "metric");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled4",
                table: "license");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled3",
                table: "ingest_type");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled2",
                table: "data_location");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled1",
                table: "connection");

            migrationBuilder.DropIndex(
                name: "IX_is_enabled",
                table: "action");

            migrationBuilder.DropColumn(
                name: "use_in_topics",
                table: "source");

            migrationBuilder.DropColumn(
                name: "use_in_topics",
                table: "series");

            migrationBuilder.DropIndex(
                name: "IX_name11",
                table: "tag");

            migrationBuilder.RenameIndex(
                name: "IX_name7",
                table: "product",
                newName: "IX_name8");

            migrationBuilder.RenameIndex(
                name: "IX_name6",
                table: "metric",
                newName: "IX_name7");

            migrationBuilder.RenameIndex(
                name: "IX_name5",
                table: "license",
                newName: "IX_name6");

            migrationBuilder.RenameIndex(
                name: "IX_name4",
                table: "ingest_type",
                newName: "IX_name5");

            migrationBuilder.RenameIndex(
                name: "IX_name3",
                table: "ingest",
                newName: "IX_name4");

            migrationBuilder.RenameIndex(
                name: "IX_name2",
                table: "data_location",
                newName: "IX_name3");

            migrationBuilder.RenameIndex(
                name: "IX_name1",
                table: "connection",
                newName: "IX_name2");

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    auto_transcribe = table.Column<bool>(type: "boolean", nullable: false),
                    category_type = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValueSql: "''"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "source_action",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValueSql: "''"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_action", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_category",
                columns: table => new
                {
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    score = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_category", x => new { x.content_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_content_category_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_category_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "source_source_action",
                columns: table => new
                {
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    source_action_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    value = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_source_action", x => new { x.source_id, x.source_action_id });
                    table.ForeignKey(
                        name: "FK_source_source_action_source_action_source_action_id",
                        column: x => x.source_action_id,
                        principalTable: "source_action",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_source_source_action_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_name13",
                table: "tone_pool",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tone_pool_owner_id",
                table: "tone_pool",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_name1",
                table: "category",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_category_category_id",
                table: "content_category",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_name11",
                table: "source_action",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_source_action_source_action_id",
                table: "source_source_action",
                column: "source_action_id");
            PostDown(migrationBuilder);
        }
    }
}
